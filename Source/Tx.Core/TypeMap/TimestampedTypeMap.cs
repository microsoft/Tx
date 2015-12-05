// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System.Collections.Generic;

    public sealed class TimestampedTypeMap<T> : IPartitionableTypeMap<Timestamped<T>, Type>
    {
        public Func<Timestamped<T>, DateTimeOffset> TimeFunction
        {
            get
            {
                return item => item.Timestamp;
            }
        }

        public IEqualityComparer<Type> Comparer
        {
            get
            {
                return EqualityComparer<Type>.Default;
            }
        }

        public Func<Timestamped<T>, object> GetTransform(Type outputType)
        {
            return item => (object)item.Value;
        }

        public Type GetTypeKey(Type outputType)
        {
            return outputType;
        }

        public Type GetInputKey(Timestamped<T> evt)
        {
            return typeof(T);
        }
    }
}
