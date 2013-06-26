// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Reactive
{
    public class CompositeDeserializer<TInput> : IObserver<TInput>, IDeserializer
    {
        private readonly List<IDeserializer<TInput>> _deserializers;
        private IObserver<Timestamped<object>> _observer;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public CompositeDeserializer(
            IObserver<Timestamped<object>> observer,
            params Type[] typeMaps)
        {
            _observer = observer;

            _deserializers = new List<IDeserializer<TInput>>();
            foreach (Type mapType in typeMaps)
            {
                object mapInstance = Activator.CreateInstance(mapType);

                Type mapInterface = mapType.GetInterfaces().FirstOrDefault(i => i.Name == typeof (IPartitionableTypeMap<,>).Name);

                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (PartitionKeyDeserializer<,>).MakeGenericType(mapInterface.GetGenericArguments());
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                mapInterface = mapType.GetInterface(typeof (IRootTypeMap<,>).Name);
                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (RootDeserializer<,>).MakeGenericType(mapInterface.GetGenericArguments());
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                mapInterface = mapType.GetInterface(typeof (ITypeMap<>).Name);
                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (TransformDeserializer<>).MakeGenericType(mapInterface.GetGenericArguments());
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                throw new Exception("The type " + mapType.FullName + " must implement one of these interfaces :"
                                    + typeof (ITypeMap<>).Name + ", "
                                    + typeof (IRootTypeMap<,>).Name + ", "
                                    + typeof (IPartitionableTypeMap<,>).Name);
            }
        }

        public void AddKnownType(Type type)
        {
            foreach (var d in _deserializers)
            {
                d.AddKnownType(type);
            }
        }

        public void SetOutput(IObserver<Timestamped<object>> observer)
        {
            _observer = observer;
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
            foreach (var d in _deserializers)
            {
                Timestamped<object> ts;
                if (d.TryDeserialize(value, out ts))
                {
                    // TODO: this achieves the right semantics, 
                    // but the performance will be sub optimal

                    if (ts.Timestamp.DateTime < StartTime)
                        return;

                    if (ts.Timestamp.DateTime > EndTime)
                        return;

                    _observer.OnNext(ts);
                    return;
                }
            }
        }
    }
}