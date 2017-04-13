using System.Collections;
using System.Collections.Generic;
using System.Reactive;

namespace System.Linq
{
    /// <summary>
    /// This is operator that executes push pipeline inside pull runtime
    /// </summary>
    /// <typeparam name="TIn">Input event type</typeparam>
    /// <typeparam name="TOut">Potpit event type</typeparam>
    class PushInsidePull<TIn, TOut> : IEnumerable<TOut> 
    {
        IEnumerable<TIn> _input;
        Func<IObservable<TIn>, IObservable<TOut>> _pushPipe;
        public PushInsidePull(IEnumerable<TIn> input, Func<IObservable<TIn>, IObservable<TOut>> pushPipe) 
        {
            _input = input;
            _pushPipe = pushPipe;
        }
        public IEnumerator<TOut> GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator(), _pushPipe);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator(), _pushPipe);
        }

        class Enumerator : IEnumerator<TOut>
        {
            PushInsidePull<TIn, TOut> _parent;
            IEnumerator<TIn> _input;

            Subject<TIn> _subject = new Subject<TIn>();
            IObservable<TOut> _pushOutput;
            PushResultHolder<TOut> _result = new PushResultHolder<TOut>();

            public Enumerator(PushInsidePull<TIn, TOut> parent, IEnumerator<TIn> input, Func<IObservable<TIn>, IObservable<TOut>> pushPipe)
            {
                _parent = parent;
                _input = input;
                _pushOutput = pushPipe(_subject); // this constructs the pipeline, but does not let it go yet
                _pushOutput.Subscribe(_result); // this enables the flow... assumind someone pushed events into the _subject
            }
            public bool MoveNext()
            {
                while (true)
                {
                    if (!_input.MoveNext())
                        return false;

                    _result.HasValue = false;
                    _subject.OnNext(_input.Current);

                    if (_result.HasValue)
                        return true;
                }
            }
            public TOut Current { get { return _result.Value; } }
            object IEnumerator.Current { get { return _result.Value; } }

            public void Dispose()
            {
                ;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        class PushResultHolder<T> : IObserver<T>
        {
            public bool HasValue = false;
            public T Value;
            public void OnNext(T value)
            {
                Value = value;
                HasValue = true;
            }
            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }
        }
    }
}
