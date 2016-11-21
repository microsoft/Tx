namespace Tx.Bond
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;

    /// <summary>
    /// Class that exposes named session of Binary ETW provider as an observable sequence of BinaryEnvelope events.
    /// </summary>
    public class BinaryEtwSessionListener : IObservable<IEnvelope>
    {
        private readonly IObservable<IEnvelope> observable;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryEtwSessionListener"/> class.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <exception cref="ArgumentNullException">sessionName is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">sessionName;Should not be empty.</exception>
        public BinaryEtwSessionListener(string sessionName)
        {
            if (sessionName == null)
            {
                throw new ArgumentNullException("sessionName");
            }

            if (string.IsNullOrEmpty(sessionName))
            {
                throw new ArgumentOutOfRangeException("sessionName", "Should not be empty.");
            }

            this.observable = BinaryEtwObservable
                .FromSession(BinaryEventSource.Log.Guid, sessionName)
                .Publish()
                .RefCount();
        }

        /// <summary>Notifies the provider that an observer is to receive notifications.</summary>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <exception cref="ArgumentNullException">observer is null.</exception>
        public IDisposable Subscribe(IObserver<IEnvelope> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException("observer");
            }

            return this.observable.Subscribe(observer);
        }
    }
}
