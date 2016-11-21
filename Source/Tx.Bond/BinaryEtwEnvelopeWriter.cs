namespace Tx.Bond
{
    using System;
    using System.Reactive;

    public class BinaryEtwEnvelopeWriter : IObserver<IEnvelope>
    {
        private readonly string sourceIdentifier;

        public BinaryEtwEnvelopeWriter(string sourceIdentifier)
        {
            this.sourceIdentifier = sourceIdentifier;
        }

        public void OnNext(IEnvelope value)
        {
            BinaryEventSource.Log.Write(
                value.ReceivedTime.DateTime,
                value.ReceivedTime.DateTime,
                value.Protocol,
                this.sourceIdentifier,
                value.Payload,
                value.TypeId);
        }

        public void OnError(Exception error)
        {
            BinaryEventSource.Log.Error(error.ToString());
        }

        public void OnCompleted()
        {
        }
    }
}