﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using Reflection;
    using System;
    using System.Collections.Generic;

    public sealed class PartitionableContravariantTypeMap : IPartitionableTypeMap<Timestamped<object>, Type>
    {
        private static readonly Type _objectType = typeof(object);

        public Func<Timestamped<object>, object> GetTransform(Type outputType)
        {
            return Transform;
        }

        public Func<Timestamped<object>, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetTimestamp;
            }
        }

        public Type GetTypeKey(Type outputType)
        {
            return outputType;
        }

        public Type GetInputKey(Timestamped<object> evt)
        {
            if (evt.Value == null)
            {
                return _objectType;
            }

            var type = evt.Value.GetType().GetTypeInfo();
            while (type.BaseType != _objectType && type.BaseType != typeof(ValueType))
            {
                type = type.BaseType.GetTypeInfo();
            }

            return type.AsType();
        }

        public IEqualityComparer<Type> Comparer
        {
            get
            {
                return EqualityComparer<Type>.Default;
            }
        }

        private static DateTimeOffset GetTimestamp(Timestamped<object> item)
        {
            return item.Timestamp;
        }

        private static object Transform(Timestamped<object> item)
        {
            return item.Value;
        }
    }
}
