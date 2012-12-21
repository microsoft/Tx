using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reactive
{
    public sealed class RootDeserializer<TInput, TOutputBase> : IDeserializer<TInput> where TOutputBase : new()
    {
        Func<TInput, DateTimeOffset> _timeFunction;
        Func<TInput, object> _transform;
        bool _rootOfInterest = false;

        public RootDeserializer(ITypeMap<TInput> typeMap)
        {
            _timeFunction = typeMap.TimeFunction;
            _transform = typeMap.GetTransform(typeof(TOutputBase));
        }

        public void AddKnownType(Type type)
        {
            if (type == typeof(TOutputBase))
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
