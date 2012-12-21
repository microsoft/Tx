namespace System.Reactive
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    
    class BufferQueue<T> : IObserver<T>, IEnumerator<T>, IDisposable
    {
        BlockingCollection<T> _queue = new BlockingCollection<T>();
        T _current;
        Exception _error;

        public void OnCompleted()
        {
            _queue.CompleteAdding();
        }

        public void OnError(Exception error)
        {
            _error = error;
            _queue.CompleteAdding();
        }

        public void OnNext(T value)
        {
            _queue.Add(value);
        }

        public T Current
        {
            get { return _current; }
        }

        public void Dispose()
        {
            _queue.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            if (_error != null)
                throw _error;

            // Bart, I tried with TryTake, and it sometimes returns immediately on empty queue (so tests fail)
            try
            {
                _current = _queue.Take();
                return true;
            }
            catch (InvalidOperationException)
            {
                if (_error != null)
                    throw _error;

                return false;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            _queue.Dispose();
        }
    }
}
