namespace Tx.Core
{
    using System;

    public interface ITransformBuilder<out TOut>
    {
        Func<TIn, TOut> Build<TIn>();
    }
}
