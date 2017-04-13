using System.Collections.Generic;

namespace System.Reactive
{
    // Cold Observable represents collection of stored events that is replayed when you call Subscribe
    // http://introtorx.com/Content/v1.0.10621.0/14_HotAndColdObservables.html#HotAndCold
    // this code illustrates the API concept, ignoring the threading (Schedulers) and cancelation (Dispose)
    class ColdObservable<T> : IObservable<T>
    {
        IEnumerable<T> _events;
        public ColdObservable(IEnumerable<T> events)
        {
            _events = events;
        }
        public IDisposable Subscribe(IObserver<T> observer)
        {
            foreach (T evt in _events)
                observer.OnNext(evt);

            observer.OnCompleted();

            return new Subscription(); 
        }

        class Subscription : IDisposable
        {
            public void Dispose()
            {
                // this example does not imlement the cancelation mechanism with Dispose
            }
        }
    }
}
