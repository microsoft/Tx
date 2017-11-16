// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public sealed class RootDeserializer<TInput, TOutputBase> : IDeserializer<TInput> where TOutputBase : new()
    {
        private readonly Func<TInput, DateTimeOffset> _timeFunction;
        private readonly Func<TInput, object> _transform;
        private bool _rootOfInterest;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDeserializer{TInput, TOutputBase}"/> class.
        /// </summary>
        /// <param name="typeMap">The instance of type map that will be used to deserialize the input.</param>
        public RootDeserializer(ITypeMap<TInput> typeMap)
        {
            _timeFunction = typeMap.TimeFunction;
            _transform = typeMap.GetTransform(typeof (TOutputBase));
        }

        public void AddKnownType(Type type)
        {
            if (type == typeof (TOutputBase))
                _rootOfInterest = true;
        }

        public bool TryDeserialize(TInput value, out Timestamped<object> ts)
        {
            if (!_rootOfInterest)
            {
                ts = default(Timestamped<object>);
                return false;
            }

            object o = _transform(value);
            ts = new Timestamped<object>(o, _timeFunction(value));
            return true;
        }
    }
}