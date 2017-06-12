// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            params ITypeMap<TInput>[] typeMaps)
        {
            _observer = observer;

            _deserializers = new List<IDeserializer<TInput>>();
            foreach (ITypeMap<TInput> mapInstance in typeMaps)
            {
                Type mapInterface = mapInstance.GetType().GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.Name == typeof(IPartitionableTypeMap<,>).Name);

                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (PartitionKeyDeserializer<,>).MakeGenericType(mapInterface.GenericTypeArguments);
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                mapInterface = mapInstance.GetType().GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.Name == typeof(IRootTypeMap<,>).Name);
                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (RootDeserializer<,>).MakeGenericType(mapInterface.GenericTypeArguments);
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                mapInterface = mapInstance.GetType().GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.Name == typeof(ITypeMap<>).Name);
                if (mapInterface != null)
                {
                    Type deserializerType =
                        typeof (TransformDeserializer<>).MakeGenericType(mapInterface.GenericTypeArguments);
                    object deserializerInstance = Activator.CreateInstance(deserializerType, mapInstance);
                    _deserializers.Add((IDeserializer<TInput>) deserializerInstance);
                    continue;
                }

                throw new Exception("The type " + mapInstance.GetType().FullName + " must implement one of these interfaces :"
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

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            _observer.OnCompleted();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(TInput value)
        {
            DateTimeOffset? timestamp = null;
            foreach (var d in _deserializers)
            {
                Timestamped<object> ts;
                if (d.TryDeserialize(value, out ts))
                {
                    if (timestamp.HasValue && timestamp.Value != ts.Timestamp)
                    {
                        _observer.OnError(new Exception("Several type maps return different timestamps for the same source event."));
                        return;
                    }

                    timestamp = ts.Timestamp;
                    // TODO: this achieves the right semantics, 
                    // but the performance will be sub optimal

                    if (ts.Timestamp.DateTime < StartTime)
                        return;

                    if (ts.Timestamp.DateTime > EndTime)
                        return;

                    _observer.OnNext(ts);
                }
            }
        }
    }
}