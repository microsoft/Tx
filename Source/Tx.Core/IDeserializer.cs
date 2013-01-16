// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reflection;

namespace System.Reactive
{
    public interface IDeserializer
    {
        void AddKnownType(Type type);
    }

    public interface IDeserializer<TInput> : IDeserializer
    {
        bool TryDeserialize(TInput value, out Timestamped<object> ts);
    }
}
