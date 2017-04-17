namespace Tx.Core
{
    using System;
    using System.Reactive;
    using System.Reflection;

    public class PassthroughTransformBuilder : ITransformBuilder<IEnvelope>
    {
        public Func<TIn, IEnvelope> Build<TIn>()
        {
            if (typeof(IEnvelope).GetTypeInfo().IsAssignableFrom(typeof(TIn)))
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
