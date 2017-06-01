// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public sealed class LambdaTypeMap<T> : SingleTypeMap<T>
    {
        private readonly Func<T, DateTimeOffset> _timestampSelector;

        public LambdaTypeMap(Func<T, DateTimeOffset> timestampSelector)
        {
            if (timestampSelector == null)
            {
                throw new ArgumentNullException(nameof(timestampSelector));
            }

            this._timestampSelector = timestampSelector;
        }

        public override Func<T, DateTimeOffset> TimeFunction
        {
            get
            {
                return this._timestampSelector;
            }
        }
    }
}
