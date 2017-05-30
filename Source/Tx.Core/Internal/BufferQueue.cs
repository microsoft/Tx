// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Reactive
{
    internal sealed class BufferQueue<T> : IObserver<T>, IEnumerator<T>
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();
        private T _current;
        private Exception _error;

        public T Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
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

        public void OnCompleted()
        {
            _queue.CompleteAdding();
        }

        public void OnError(Exception error)
        {
            _error = error;
            _queue.CompleteAdding();
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(T value)
        {
            _queue.Add(value);
        }

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}