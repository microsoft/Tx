using System;

namespace System.Reactive
{
    /// <summary>
    /// Mockup of the Select operator in Rx
    /// </summary>
    /// <typeparam name="TIn">input event type</typeparam>
    /// <typeparam name="TOut">output event type</typeparam>
    class Select<TIn, TOut> : IObserver<TIn>, IObservable<TOut>
    {
        IObservable<TIn> _source;
        Func<TIn, TOut> _transform;
        Subject<TOut> _subject;
        public Select(IObservable<TIn> source, Func<TIn, TOut> transform)
        {
            _source = source;
            _transform = transform;
            _subject = new Subject<TOut>(); // Rx used Subjects like this before the perf improvements in V2.0
        }

        public void OnNext(TIn value)
        {
            var t = _transform(value);
            _subject.OnNext(t);
        }
        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var subscription = _subject.Subscribe(observer);
            _source.Subscribe(this);
            return subscription;
        }
    }
}
