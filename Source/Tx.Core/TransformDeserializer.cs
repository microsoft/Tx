// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public sealed class TransformDeserializer<TInput> : IDeserializer<TInput>
    {
        private readonly Func<TInput, DateTimeOffset> _timeFunction;
        private readonly Func<TInput, object> _transform;
        private bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformDeserializer{TInput}"/> class.
        /// </summary>
        /// <param name="typeMap">The instance of type map that will be used to deserialize the input of type <see>
        ///     <cref>TInput</cref>
        /// </see>.</param>
        public TransformDeserializer(ITypeMap<TInput> typeMap)
        {
            _timeFunction = typeMap.TimeFunction;
            _transform = typeMap.GetTransform(typeof (TInput));
        }

        public void AddKnownType(Type type)
        {
            if (type == typeof (TInput))
                _enabled = true;
        }

        public bool TryDeserialize(TInput value, out Timestamped<object> ts)
        {
            if (!_enabled)
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