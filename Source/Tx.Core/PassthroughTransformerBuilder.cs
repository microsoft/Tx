namespace Tx.Core
{
    using System;
    using System.Reactive;

    public class PassthroughTransformerBuilder : ITransformerBuilder<IEnvelope>
    {
        public Func<TIn, IEnvelope> Build<TIn>()
        {
            if (typeof(IEnvelope).IsAssignableFrom(typeof(TIn)))
            {
                return Transform<TIn>;
            }

            return null;
        }

        private static IEnvelope Transform<TIn>(TIn item)
        {
            return item as IEnvelope;
        }
    }

}
