// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive
{
    internal sealed class OutputPump<T> : IDisposable
    {
        private readonly IEnumerator<T> _source;
        private readonly IObserver<T> _target;
        private readonly Task _thread;
        private readonly WaitHandle _waitStart;
        private long _eventsRead;

        public OutputPump(IEnumerable<T> source, IObserver<T> target, WaitHandle waitStart)
        {
            _source = source.GetEnumerator();
            _target = target;
             _waitStart = waitStart;
            _thread = Task.Run((Action)ThreadProc);
        }

        public void Dispose()
        {
            _waitStart.Dispose();
        }

        private void ThreadProc()
        {
            _waitStart.WaitOne();
            while (true)
            {
                try
                {
                    if (!_source.MoveNext())
                        break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    try
                    {
                        _target.OnError(ex);
                    }
                    catch
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
        }
    }
}