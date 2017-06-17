// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Reactive
{
    public sealed class PartitionKeyDeserializer<TInput, TKey> : IDeserializer<TInput>
    {
        private readonly Dictionary<TKey, Type> _knownTypes;
        private readonly Func<TInput, DateTimeOffset> _timeFunction;
        private readonly Dictionary<TKey, Func<TInput, object>> _transforms;
        private readonly IPartitionableTypeMap<TInput, TKey> _typeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionKeyDeserializer{TInput, TKey}"/> class.
        /// </summary>
        /// <param name="typeMap">The instance of type map that will be used to deserialize the input of type <see>
        ///     <cref>TInput</cref>
        /// </see>.</param>
        public PartitionKeyDeserializer(IPartitionableTypeMap<TInput, TKey> typeMap)
        {
            _typeMap = typeMap;
            _transforms = new Dictionary<TKey, Func<TInput, object>>(_typeMap.Comparer);
            _timeFunction = _typeMap.TimeFunction;
            _knownTypes = new Dictionary<TKey, Type>(_typeMap.Comparer);
        }

        public void AddKnownType(Type type)
        {
            TKey key = _typeMap.GetTypeKey(type);
            if (key == null || key.Equals(default(TKey)))
                return; // this deserializer does not recognize the type

            if (_knownTypes.ContainsKey(key))
                return; // adding types is idempotent operation

            _knownTypes.Add(key, type);
            _transforms.Add(key, null); // postpone the compilation until event occurence
        }

        public bool TryDeserialize(TInput value, out Timestamped<object> ts)
        {
            TKey key = _typeMap.GetInputKey(value);
            ts = default(Timestamped<object>);

            Func<TInput, object> transform;
            if (key == null || !_transforms.TryGetValue(key, out transform))
                return false;

            if (transform == null)
            {
                Type type = _knownTypes[key];
                transform = _typeMap.GetTransform(type);
                if (transform == null)
                    return false;

                _transforms[key] = transform;
            }

            object o = transform(value);

            if (o != null)
            {
                ts = new Timestamped<object>(o, _timeFunction(value));
                return true;
            }

            return false;
        }
    }
}