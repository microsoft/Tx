namespace Tx.Bond
{
    using System;
    using System.Reactive;

    using Tx.Core;

    public class SimpleWriter : Transformer<object>
    {
        public SimpleWriter(IObserver<IEnvelope> next)
            : base(
                next, 
                new PassthroughTransformerBuilder(),
                new BondCompactBinaryTransformerBuilder(),
                new JsonTransformerBuilder())
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
        }
    }
}