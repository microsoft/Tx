using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;

namespace System.Reactive
{
    public class CompositeDeserializer<TInput> : IObserver<TInput>, IDeserializer
    {
        List<IDeserializer<TInput>> _deserializers;
        IObserver<Timestamped<object>> _observer;

        public CompositeDeserializer(
            IObserver<Timestamped<object>> observer,
            params Type[] typeMaps)
        {
            _observer = observer;
            _deserializers = new List<IDeserializer<TInput>>();
            foreach (Type mapType in typeMaps)
            {
                Type mapInterface = null;
                var mapInstance = Activator.CreateInstance(mapType);

                foreach (Type i in mapType.GetInterfaces())
                {
                    if (i.Name == typeof(IPartitionableTypeMap<,>).Name)
                    {
                        mapInterface = i;
                        break;
                    }
                }

                if (mapInterface != null)
                {
                    var deserializerType = typeof(PartitionKeyDeserializer<,>).MakeGenericType(mapInterface.GetGenericArguments());
                    var deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>)deserializerInstance);
                    continue;
                }

                mapInterface = mapType.GetInterface(typeof(IRootTypeMap<,>).Name);
                if (mapInterface != null)
                {
                    var deserializerType = typeof(RootDeserializer<,>).MakeGenericType(mapInterface.GetGenericArguments());
                    var deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>)deserializerInstance);
                    continue;
                }

                mapInterface = mapType.GetInterface(typeof(ITypeMap<>).Name);
                if (mapInterface != null)
                {
                    var deserializerType = typeof(TransformDeserializer<>).MakeGenericType(mapInterface.GetGenericArguments());
                    var deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>)deserializerInstance);
                    continue;
                }

                throw new Exception("The type " + mapType.FullName + " must implement one of these interfaces :"
                    + typeof(ITypeMap<>).Name + ", "
                    + typeof(IRootTypeMap<,>).Name + ", "
                    + typeof(IPartitionableTypeMap<,>).Name);
            }
        }

        public void AddKnownType(Type type)
        {
            foreach (var d in _deserializers)
            {
                d.AddKnownType(type);
            }
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnNext(TInput value)
        {
            Timestamped<object> ts;
            foreach (IDeserializer<TInput> d in _deserializers)
            {
                if (d.TryDeserialize(value, out ts))
                {
                    _observer.OnNext(ts);
                    return;
                }
            }
        }
    }
}
