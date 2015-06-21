// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive.TypeMap
{
    public sealed class TimestampedTypeMap<T> : IRootTypeMap<Timestamped<T>, T>
    {
        public Func<Timestamped<T>, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetTimestamp;
            }
        }

        public Func<Timestamped<T>, object> GetTransform(Type outputType)
        {
            return Transform;
        }

        private static DateTimeOffset GetTimestamp(Timestamped<T> item)
        {
            return item.Timestamp;
        }

        private static object Transform(Timestamped<T> item)
        {
            return item.Value;
        }
    }
}
