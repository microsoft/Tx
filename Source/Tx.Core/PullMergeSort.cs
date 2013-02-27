// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public partial class PullMergeSort<T> : IEnumerable<T>
    {
        Func<T, DateTime> _keyFunction;
        List<IEnumerator<T>> _inputs;

        public PullMergeSort(Func<T, DateTime> keyFunction, IEnumerable<IEnumerator<T>> inputs)
        {
            _keyFunction = keyFunction;
            _inputs = new List<IEnumerator<T>>(inputs);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this, _inputs);
        }

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        class Enumerator : IEnumerator<T>
        {
            PullMergeSort<T> _parent;
            List<Reader> _inputs;
            T _current;
            bool _initialized = false;

            public Enumerator(PullMergeSort<T> parent, IEnumerable<IEnumerator<T>> inputs)
            {
                _parent = parent;
                _inputs = new  List<Reader>();
                foreach(var i in inputs)
                {
                    _inputs.Add(new Reader(i));
                }
            }

            public bool MoveNext()
            {
                if (!_initialized)
                {
                    foreach (var reader in _inputs)
                    {
                        reader.ReadOne();
                    };

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
                foreach (var input in _inputs)
                {
                    input.Dispose();
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            Reader FindStreamToRead()
            {
                Reader streamToRead = null;
                DateTime earliestTimestamp = DateTime.MaxValue;
                List<Reader> toRemove = new List<Reader>();

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

                foreach (var r in toRemove)
                {
                    _inputs.Remove(r);
                    r.Dispose();
                }

                return streamToRead;
            }
        }

        class Reader : IDisposable
        {
            IEnumerator<T> _enumerator;
            bool _isCompleted;

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

            public void  Dispose()
            {
 	            _enumerator.Dispose();
            }

            public void  ReadOne()
            {
                _isCompleted = !_enumerator.MoveNext();
            }
        }
    }
}
