using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// This is mock-up implementation of .Where. It is called Filter to avoid name collision
    /// </summary>
    class Filter<T> : IEnumerable<T>
    {
        Func<T, bool> _filter;
        IEnumerable<T> _input;

        public Filter(IEnumerable<T> input, Func<T, bool> filter)
        {
            _input = input;
            _filter = filter;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator());
        }

        class Enumerator : IEnumerator<T>
        {
            Filter<T> _parent;
            IEnumerator<T> _input;
            public Enumerator(Filter<T> parent, IEnumerator<T> input)
            {
                _parent = parent;
                _input = input;
            }
            public bool MoveNext()
            {
                while (true)
                {
                    if (!_input.MoveNext())
                        return false;

                    if (_parent._filter(_input.Current))
                        return true;
                }
            }
            public T Current  { get { return _input.Current; } }
            object IEnumerator.Current { get { return _input.Current; } }

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
