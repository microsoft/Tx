// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;

    public abstract class SingleTypeMap<T> : ITypeMap<T>
    {
        public abstract Func<T, DateTimeOffset> TimeFunction { get; }

        public Func<T, object> GetTransform(Type outputType)
        {
            if (outputType == typeof(T))
            {
                return envelope => envelope;
            }
            else
            {
                return envelope => null;
            }
        }
    }
}
