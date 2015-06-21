// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public interface IDeserializer
    {
        void AddKnownType(Type type);
    }

    public interface IDeserializer<in TInput> : IDeserializer
    {
        bool TryDeserialize(TInput value, out Timestamped<object> ts);
    }
}