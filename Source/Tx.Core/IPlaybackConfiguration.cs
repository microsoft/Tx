// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace System.Reactive
{
    public interface IPlaybackConfiguration
    {
        void AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params Type[] typeMaps);

        void AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params ITypeMap<TInput>[] typeMaps);
    }
}