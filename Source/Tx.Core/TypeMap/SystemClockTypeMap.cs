// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public sealed class SystemClockTypeMap<T> : SingleTypeMap<T>
    {
        public override Func<T, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetUtcNow;
            }
        }

        private static DateTimeOffset GetUtcNow(T item)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
