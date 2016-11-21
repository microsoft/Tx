namespace Tx.Bond
{
    using System;
    using System.Reactive;

    public class BinaryEventSourceObserver : IObserver<IEnvelope>
    {
        public void OnNext(IEnvelope item)
        {
            BinaryEventSource.Log.WriteInternal(item);
        }

        public void OnError(Exception error)
        {
            BinaryEventSource.Log.Error(error.ToString());
        }

        public void OnCompleted()
        {
        }
    }

    public class BinaryEtwWriter : SimpleWriter
    {
        public BinaryEtwWriter()
            : base(new BinaryEventSourceObserver())
        {
        }
    }
}
