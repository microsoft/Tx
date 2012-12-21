using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tx.Windows
{    
    /// <summary>
    /// Observable that will read .etl files and push the 
    /// and produce the events in the ETW native format
    /// </summary>
    class EtwFileReader : IDisposable
    {
        readonly Guid system = new Guid("{68fdd900-4a3e-11d1-84f4-0000f80464e3}");

        IObserver<EtwNativeEvent> _observer;
        EVENT_TRACE_LOGFILE[] _logFiles;
        GCHandle[] _logFileHandles;
        Thread _thread;
        ulong[] _handles;
        bool _disposed;
        EtwNativeEvent _evt;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observer">Observer to push events into</param>
        /// <param name="etlFiles">.etl (Event Trace Log) files to read. Up to 63 files are supported</param>
        public EtwFileReader(IObserver<EtwNativeEvent> observer, params string[] etlFiles)
        {
            _observer = observer;

            // pin the strings in memory, allowing pointers to be passed in the event callback
            _logFiles = new EVENT_TRACE_LOGFILE[etlFiles.Length];
            _logFileHandles = new GCHandle[etlFiles.Length];
            for (int i = 0; i < _logFileHandles.Length; i++)
            {
                _logFiles[i] = new EVENT_TRACE_LOGFILE
                {
                    ProcessTraceMode = EtwNativeMethods.TraceModeEventRecord,
                    LogFileName = Path.GetFullPath(etlFiles[i]),
                    EventRecordCallback = EtwCallback
                };
                _logFileHandles[i] = GCHandle.Alloc(_logFiles[i]);
            }

            _thread = new Thread(ThreadProc);
            _thread.Name = "EtwFileObservable";
            _thread.Start();
        }

        void ThreadProc()
        {
            int error = 0;
            _handles = new ulong[_logFiles.Length];
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
                    else
                    {
                        _observer.OnError(new Win32Exception(error));
                        return;
                    }
                }
            }

            try
            {
                error = EtwNativeMethods.ProcessTrace(_handles, (uint)_handles.Length, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }
            if (error != 0)
            {
                _observer.OnError(new Win32Exception(error));
                return;
            }

            _observer.OnCompleted();
        }

        unsafe void EtwCallback(ref EVENT_RECORD record)
        {
            if (record.EventHeader.ProviderId == system)
                return;

            fixed (EVENT_RECORD* p = &record)
            {
                EtwNativeEvent evt;
                evt.record = p;
                evt._data = (byte*)record.UserData.ToPointer();
                evt._end = evt._data + record.UserDataLength;
                evt._length = 0;
                _observer.OnNext(evt);
            }
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

        ~EtwFileReader()
        {
            Dispose();
        }
    }
}
