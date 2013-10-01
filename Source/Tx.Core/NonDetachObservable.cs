// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    /// <summary>
    /// This is like AnonimosObservable in Rx, but without the auto-detach logic
    /// </summary>
    public class NonDetachObservable<T> : IObservable<T>
    {
        Func<IObserver<T>, IDisposable> _subscribe;

        public NonDetachObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subscribe(observer);
        }
    }
}
