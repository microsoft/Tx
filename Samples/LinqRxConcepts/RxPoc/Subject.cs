using System.Collections.Generic;

namespace System.Reactive
{
    /// <summary>
    /// Mockup of the Subject in Rx
    /// Note the operators in Rx don't include subjects nowadays. 
    /// Using subject in each operator is how Rx worked in v1. In v2.0 these were removed for better performance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Subject<T> : IObserver<T>, IObservable<T>
    {
        List<Subscription> _subscriptions = new List<Subscription>();
        public void OnNext(T value)
        {
            foreach (var s in _subscriptions) s.Subscriber.OnNext(value);
        }
        public void OnCompleted()
        {
            foreach (var s in _subscriptions) s.Subscriber.OnCompleted();
        }

        public void OnError(Exception error)
        {
            foreach (var s in _subscriptions) s.Subscriber.OnError(error);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var s = new Subscription() { Parent = this, Subscriber = observer };
            _subscriptions.Add(s);
            return (s);
        }

        void Unsubscribe(Subscription s)
        {
            _subscriptions.Remove(s);
        }

        class Subscription : IDisposable
        {
            public IObserver<T> Subscriber;
            public Subject<T> Parent;
            public void Dispose()
            {
                Parent.Unsubscribe(this);
            }
        }
    }
}
