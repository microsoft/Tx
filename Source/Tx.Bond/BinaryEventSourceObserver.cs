namespace Tx.Bond
{
    using System;
    using System.Reactive;

    public class BinaryEventSourceObserver : IObserver<IEnvelope>
    {
        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(IEnvelope value)
        {
            BinaryEventSource.Log.WriteInternal(value);
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            BinaryEventSource.Log.Error(error.ToString());
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
        }
    }

    public class BinaryEtwWriter : BondJsonConverter
    {
        public BinaryEtwWriter()
            : base(new BinaryEventSourceObserver())
        {
        }
    }
}
