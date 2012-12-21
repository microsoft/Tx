using System.Collections.Generic;

namespace System.Reactive
{

    public interface ITypeMap<TInput>
    {
        Func<TInput, DateTimeOffset> TimeFunction { get; }
        Func<TInput, object> GetTransform(Type outputType);
    }

    public interface IRootTypeMap<TInput, TOutputBase> : ITypeMap<TInput>
    {
    }

    public interface IPartitionableTypeMap<TInput, TKey> : ITypeMap<TInput>
    {
        IEqualityComparer<TKey> Comparer { get; }
        TKey GetTypeKey(Type outputType);
        TKey GetInputKey(TInput evt);
     }
}
