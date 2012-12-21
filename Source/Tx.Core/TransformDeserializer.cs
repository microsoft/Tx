namespace System.Reactive
{
    public sealed class TransformDeserializer<TInput> : IDeserializer<TInput>
    {
        Func<TInput, DateTimeOffset> _timeFunction;
        Func<TInput, object> _transform;
        bool _enabled = false;

        public TransformDeserializer(ITypeMap<TInput> typeMap)
        {
            _timeFunction = typeMap.TimeFunction;
            _transform = typeMap.GetTransform(typeof(TInput));
        }

        public void AddKnownType(Type type)
        {
            if (type == typeof(TInput))
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
