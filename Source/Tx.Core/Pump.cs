namespace System.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    // BUGBUG: The Pump seems something that should exist in Rx
    // Exposing this here as public is weird
    public abstract class Pump
    {
        protected ManualResetEvent _completed = new ManualResetEvent(false);

        public WaitHandle Completed { get { return _completed; } }
    }

    class OutputPump<T> : Pump, IDisposable
    {
        IEnumerator<T> _source;
        IObserver<T> _target;
        Thread _thread;
        WaitHandle _waitStart;
        long _eventsRead;

        public OutputPump(IEnumerable<T> source, IObserver<T> target, WaitHandle waitStart)
        {
            _source = source.GetEnumerator();
            _target = target;
            _waitStart = waitStart;
            _thread = new Thread(ThreadProc);
            _thread.Name = "Pump " + typeof(T).Name;
            _thread.Start();
        }

        public void Dispose()
        {
            _thread.Abort();
            _waitStart.Dispose();
        }

        void ThreadProc()
        {
            _waitStart.WaitOne();
            while (true)
            {
                try
                {
                    if (!_source.MoveNext())
                        break;
                }
                catch (Exception ex)
                {
                    try
                    {
                        _target.OnError(ex);
                    }
                    catch (Exception)
                    {
                    }

                    break;
                }

                _eventsRead++;

                try
                {
                    _target.OnNext(_source.Current);
                }
                catch (Exception ex)
                {
                    _target.OnError(ex);
                }
            }

            _target.OnCompleted();
            _completed.Set();
        }
    }
}
