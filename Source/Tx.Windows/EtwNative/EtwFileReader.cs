// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tx.Windows
{
    /// <summary>
    ///     Observable that will read .etl files and push the
    ///     and produce the events in the ETW native format
    /// </summary>
    internal class EtwFileReader : IDisposable
    {
        private readonly GCHandle[] _logFileHandles;
        private readonly EVENT_TRACE_LOGFILE[] _logFiles;
        private readonly IObserver<EtwNativeEvent> _observer;
        private readonly Thread _thread;
        private readonly Guid _system = new Guid("{68fdd900-4a3e-11d1-84f4-0000f80464e3}");
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private bool _disposed;
        private ulong[] _handles;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observer">Observer to push events into</param>
        /// <param name="etlFiles">.etl (Event Trace Log) files to read. Up to 63 files are supported</param>
        public EtwFileReader(IObserver<EtwNativeEvent> observer, params string[] etlFiles) :
            this(observer, false, DateTime.MinValue, DateTime.MaxValue, etlFiles)
        {
        }

        public EtwFileReader(IObserver<EtwNativeEvent> observer, bool sequential, params string[] etlFiles) :
            this(observer, sequential, DateTime.MinValue, DateTime.MaxValue, etlFiles)
        { 
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observer">Observer to push events into</param>
        /// <param name="sequential">set sequential to true to sequentially stream the logs</param>
        /// <param name="startTime">start time for the events from logs</param>
        /// <param name="endTime">end time for the events from logs</param>
        /// <param name="etlFiles">.etl (Event Trace Log) files to read. Up to 63 files are supported in non sequential mode.
        /// Theoritically no limits on number of files in sequential mode.</param>
        public EtwFileReader(IObserver<EtwNativeEvent> observer, bool sequential, DateTime startTime, DateTime endTime, params string[] etlFiles)
        {
            _observer = observer;
            _startTime = startTime;
            _endTime = endTime;

            // pin the strings in memory, allowing pointers to be passed in the event callback
            _logFiles = new EVENT_TRACE_LOGFILE[etlFiles.Length];
            _logFileHandles = new GCHandle[etlFiles.Length];
            for (int i = 0; i < _logFileHandles.Length; i++)
            {
                _logFiles[i] = new EVENT_TRACE_LOGFILE
                {
                    ProcessTraceMode = EtwNativeMethods.ProcessTraceModeEventRecord,
                    LogFileName = Path.GetFullPath(etlFiles[i]),
                    EventRecordCallback = EtwCallback
                };
                _logFileHandles[i] = GCHandle.Alloc(_logFiles[i]);
            }

            if (sequential == true)
            {
                _thread = new Thread(ProcessTracesInSequence) { Name = "EtwFileObservable" };
            }
            else
            {
                _thread = new Thread(MergeTracesAndProcess) { Name = "EtwFileObservable" };
            }
            _thread.Start();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                for (int i = 0; i < _handles.Length; i++)
                {
                    EtwNativeMethods.CloseTrace(_handles[i]);
                    _logFileHandles[i].Free();
                }
            }
        }

        private static bool TryConvertToFILETIME(DateTime dateTime, ref System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            if (dateTime == DateTime.MinValue)
                return false;

            if (dateTime == DateTime.MaxValue)
                return false;

            long lfileTime = dateTime.ToFileTime();

            fileTime.dwHighDateTime = (int)(lfileTime >> 32);
            fileTime.dwLowDateTime = (int)(lfileTime & 0xFFFFFFFF); 

            return true;
        }

        private static IntPtr ConvertDateTime(DateTime dateTime)
        {
           System.Runtime.InteropServices.ComTypes.FILETIME fileTime = 
                new System.Runtime.InteropServices.ComTypes.FILETIME();

            if (TryConvertToFILETIME(dateTime, ref fileTime))
            {
                int sizeOfFileTime = Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.FILETIME));

                IntPtr fileTimePtr = Marshal.AllocHGlobal(sizeOfFileTime);
                Marshal.StructureToPtr(fileTime, fileTimePtr, false);

                return fileTimePtr;
            }

            return IntPtr.Zero;
        }

        private void ProcessTracesInSequence()
        {
            int error = 0;
            _handles = new ulong[_logFiles.Length];
            IntPtr startTime = ConvertDateTime(_startTime);
            IntPtr endTime = ConvertDateTime(_endTime);

            try
            {
                for (int i = 0; i < _logFiles.Length; i++)
                {
                    _handles[i] = EtwNativeMethods.OpenTrace(ref _logFiles[i]);

                    if (_handles[i] == EtwNativeMethods.InvalidHandle)
                    {
                        error = Marshal.GetLastWin32Error();
                        if (error == EtwNativeMethods.ErrorNotFound)
                        {
                            _observer.OnError(new FileNotFoundException("Could not find file " + _logFiles[i].LogFileName));
                            return;
                        }

                        _observer.OnError(new Win32Exception(error));
                        return;
                    }

                    error = EtwNativeMethods.ProcessTrace(new ulong[] { _handles[i] }, (uint)1, startTime, endTime);
                }
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }
            finally
            {
                if (startTime != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(startTime);
                    startTime = IntPtr.Zero;
                }

                if (endTime != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(endTime);
                    endTime = IntPtr.Zero;
                }
            }

            if (error != 0)
            {
                _observer.OnError(new Win32Exception(error));
                return;
            }

            _observer.OnCompleted();
        }

        private void MergeTracesAndProcess()
        {
            int error;
            _handles = new ulong[_logFiles.Length];
            IntPtr startTime = ConvertDateTime(_startTime);
            IntPtr endTime = ConvertDateTime(_endTime);

            for (int i = 0; i < _logFiles.Length; i++)
            {
                _handles[i] = EtwNativeMethods.OpenTrace(ref _logFiles[i]);

                if (_handles[i] == EtwNativeMethods.InvalidHandle)
                {
                    error = Marshal.GetLastWin32Error();
                    if (error == EtwNativeMethods.ErrorNotFound)
                    {
                        _observer.OnError(new FileNotFoundException("Could not find file " + _logFiles[i].LogFileName));
                        return;
                    }

                    _observer.OnError(new Win32Exception(error));
                    return;
                }
            }

            try
            {
                error = EtwNativeMethods.ProcessTrace(_handles, (uint)_handles.Length, startTime, endTime);
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }
            finally
            {
                if (startTime != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(startTime);
                    startTime = IntPtr.Zero;
                }

                if (endTime != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(endTime);
                    endTime = IntPtr.Zero;
                }
            }

            if (error != 0)
            {
                _observer.OnError(new Win32Exception(error));
                return;
            }

            _observer.OnCompleted();
        }

        private unsafe void EtwCallback(ref EVENT_RECORD record)
        {
            if (record.EventHeader.ProviderId == _system)
                return;

            fixed (EVENT_RECORD* p = &record)
            {
                EtwNativeEvent evt;
                evt.record = p;
                evt._data = (byte*) record.UserData.ToPointer();
                evt._end = evt._data + record.UserDataLength;
                evt._length = 0;
                _observer.OnNext(evt);
            }
        }

        ~EtwFileReader()
        {
            Dispose();
        }
    }
}