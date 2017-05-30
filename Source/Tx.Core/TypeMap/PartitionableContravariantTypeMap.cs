// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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

            var type = evt.Value.GetType();
            while (type.GetTypeInfo().BaseType != _objectType && type.GetTypeInfo().BaseType != typeof(ValueType))
            {
                type = type.GetTypeInfo().BaseType;
            }

            return type;
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
