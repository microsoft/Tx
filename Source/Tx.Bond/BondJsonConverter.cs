namespace Tx.Bond
{
    using System;
    using System.Reactive;

    using Tx.Core;

    public class BondJsonConverter : Tx.Core.Converter<object, IEnvelope>
    {
        public BondJsonConverter(IObserver<IEnvelope> next)
            : base(
                next, 
                new PassthroughTransformBuilder(),
                new BondCompactBinaryTransformBuilder(),
                new JsonTransformBuilder())
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
        }
    }
}