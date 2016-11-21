namespace Tx.Core
{
    using System;

    public interface ITransformerBuilder<out TOut>
    {
        Func<TIn, TOut> Build<TIn>();
    }
}
