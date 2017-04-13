using System;

namespace System.Reactive
{
    /// <summary>
    /// Mockup of the Where operator in Rx
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    class Where<T> : IObserver<T>, IObservable<T>
    {
        IObservable<T> _source;
        Func<T, bool> _filter;
        Subject<T> _subject;  
        public Where(IObservable<T> source, Func<T, bool> filter)
        {
            _source = source;
            _filter = filter;
            _subject = new Subject<T>(); // Rx used Subjects like this before the perf improvements in V2.0
        }

        public void OnNext(T value)
        {
            if (_filter(value))
                _subject.OnNext(value);
        }
        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var subscription = _subject.Subscribe(observer);
            _source.Subscribe(this);
            return subscription;
        }
    }
}
