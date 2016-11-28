namespace Tx.Bond
{
    using System;
    using System.Reactive;

    using Tx.Core;

    public class SimpleWriter : Tx.Core.Converter<object, IEnvelope>
    {
        public SimpleWriter(IObserver<IEnvelope> next)
            : base(
                next, 
                new PassthroughTransformBuilder(),
                new BondCompactBinaryTransformBuilder(),
                new JsonTransformBuilder())
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
        }
    }
}