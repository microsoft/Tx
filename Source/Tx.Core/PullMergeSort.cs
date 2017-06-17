// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace System.Reactive
{
    public class PullMergeSort<T> : IEnumerable<T>
    {
        private readonly List<IEnumerator<T>> _inputs;
        private readonly Func<T, DateTime> _keyFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullMergeSort{T}"/> class.
        /// </summary>
        /// <param name="keyFunction">Time stamp getter function.</param>
        /// <param name="inputs">The collection of sequences of <see>
        ///         <cref>T</cref>
        ///     </see>
        ///     elements.</param>
        public PullMergeSort(Func<T, DateTime> keyFunction, IEnumerable<IEnumerator<T>> inputs)
        {
            _keyFunction = keyFunction;
            _inputs = new List<IEnumerator<T>>(inputs);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this, _inputs);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly List<Reader> _inputs;
            private readonly PullMergeSort<T> _parent;
            private T _current;
            private bool _initialized;

            public Enumerator(PullMergeSort<T> parent, IEnumerable<IEnumerator<T>> inputs)
            {
                _parent = parent;
                _inputs = new List<Reader>();
                foreach (var i in inputs)
                {
                    _inputs.Add(new Reader(i));
                }
            }

            public bool MoveNext()
            {
                if (!_initialized)
                {
                    foreach (Reader reader in _inputs)
                    {
                        reader.ReadOne();
                    }

                    _initialized = true;
                }

                Reader streamToRead = FindStreamToRead();

                if (streamToRead == null)
                    return false;

                _current = streamToRead.Next;
                streamToRead.ReadOne();
                return true;
            }

            public T Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                foreach (Reader input in _inputs)
                {
                    input.Dispose();
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            private Reader FindStreamToRead()
            {
                Reader streamToRead = null;
                DateTime earliestTimestamp = DateTime.MaxValue;
                var toRemove = new List<Reader>();

                foreach (Reader s in _inputs)
                {
                    if (s.IsCompleted)
                    {
                        toRemove.Add(s);
                        continue;
                    }

                    DateTime timestamp = _parent._keyFunction(s.Next);
                    if (timestamp < earliestTimestamp)
                    {
                        earliestTimestamp = timestamp;
                        streamToRead = s;
                    }
                }

                foreach (Reader r in toRemove)
                {
                    _inputs.Remove(r);
                    r.Dispose();
                }

                return streamToRead;
            }
        }

        private sealed class Reader : IDisposable
        {
            private readonly IEnumerator<T> _enumerator;
            private bool _isCompleted;

            public Reader(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public bool IsCompleted
            {
                get { return _isCompleted; }
            }

            public T Next
            {
                get { return _enumerator.Current; }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public void ReadOne()
            {
                _isCompleted = !_enumerator.MoveNext();
            }
        }
    }
}