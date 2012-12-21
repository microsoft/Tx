using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Tx.Windows
{
    /// <summary>
    /// Listener to real-time event tracing session
    /// </summary>
    class EtwListener : IDisposable
    {
        IObserver<EtwNativeEvent> _observer;
        EVENT_TRACE_LOGFILE _logFile;
        ulong _handle;
        Thread _thread;
        bool _disposed;

        /// <summary>
        /// Constructor
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
                    ProcessTraceMode = EtwNativeMethods.TraceModeRealTime | EtwNativeMethods.TraceModeEventRecord,
                    LoggerName = sessionName,
                    EventRecordCallback = EtwCallback
                };
                
            _thread = new Thread(ThreadProc);
            _thread.Name = "EtwSession " + sessionName;
            _thread.Start();
        }

        void ThreadProc()
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
                error = EtwNativeMethods.ProcessTrace(new ulong[] { _handle }, 1, IntPtr.Zero, IntPtr.Zero);
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
                EtwNativeMethods.CloseTrace(_handle);
                _thread.Join();
            }
        }
    }
}
