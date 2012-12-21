using System.Collections.Generic;

namespace System.Reactive
{
    public sealed class PartitionKeyDeserializer<TInput, TKey> : IDeserializer<TInput>
    {
        IPartitionableTypeMap<TInput, TKey> _typeMap;
        Func<TInput, DateTimeOffset> _timeFunction;
        Dictionary<TKey, Func<TInput, object>> _transforms;
        Dictionary<TKey, Type> _knownTypes;

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

            Func<TInput, object> transform;
            if (!_transforms.TryGetValue(key, out transform))
            {
                ts = default(Timestamped<object>);
                return false;
            }

            if (transform == null)
            {
                Type type = _knownTypes[key];
                transform = _typeMap.GetTransform(type);
                _transforms[key] = transform;
            }

            object o = transform(value);
            ts = new Timestamped<object>(o, _timeFunction(value));
            return true;
        }
    }
}
