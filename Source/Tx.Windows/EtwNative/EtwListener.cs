// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Tx.Windows
{
    /// <summary>
    ///     Listener to real-time event tracing session
    /// </summary>
    internal class EtwListener : IDisposable
    {
        private readonly IObserver<EtwNativeEvent> _observer;
        private readonly Thread _thread;
        private bool _disposed;
        private ulong _handle;
        private EVENT_TRACE_LOGFILE _logFile;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observer">Observer to push events into</param>
        /// <param name="sessionName">real-time session name</param>
        public EtwListener(IObserver<EtwNativeEvent> observer, string sessionName)
        {
            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            // I don't know how to check for "Performance Log Users" group
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                throw new Exception("To use ETW real-time session, you have to be Administrator");

            _observer = observer;
            _logFile = new EVENT_TRACE_LOGFILE
                {
                    ProcessTraceMode = EtwNativeMethods.EventTraceRealTimeMode | EtwNativeMethods.ProcessTraceModeEventRecord,
                    LoggerName = sessionName,
                    EventRecordCallback = EtwCallback
                };

            _thread = new Thread(ThreadProc);
            _thread.Name = "EtwSession " + sessionName;
            _thread.Start();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                EtwNativeMethods.CloseTrace(_handle);

                // the above causes EtwNativeMethods.OpenTrace to return sucessfuly
                // and the thread which invokes the callbacks to finish
                _thread.Join();
            }
        }

        private void ThreadProc()
        {
            int error;

            _handle = EtwNativeMethods.OpenTrace(ref _logFile);

            if (_handle == EtwNativeMethods.InvalidHandle)
            {
                error = Marshal.GetLastWin32Error();
                if (error == EtwNativeMethods.ErrorNotFound)
                {
                    _observer.OnError(new Exception("Could not find ETW real-time session " + _logFile.LoggerName));
                    return;
                }
                else
                {
                    _observer.OnError(new Win32Exception(error));
                    return;
                }
            }

            try
            {
                error = EtwNativeMethods.ProcessTrace(new[] {_handle}, 1, IntPtr.Zero, IntPtr.Zero);
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

        private unsafe void EtwCallback(ref EVENT_RECORD record)
        {
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
    }
}