// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Reactive
{
    public interface ITypeMap<in TInput>
    {
        Func<TInput, DateTimeOffset> TimeFunction { get; }
        Func<TInput, object> GetTransform(Type outputType);
    }

    public interface IRootTypeMap<in TInput, TOutputBase> : ITypeMap<TInput>
    {
    }

    public interface IPartitionableTypeMap<in TInput, TKey> : ITypeMap<TInput>
    {
        IEqualityComparer<TKey> Comparer { get; }
        TKey GetTypeKey(Type outputType);
        TKey GetInputKey(TInput evt);
    }
}