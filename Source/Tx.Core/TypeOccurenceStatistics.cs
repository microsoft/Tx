// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Threading;
using System.Reflection;

namespace System.Reactive
{
    public class TypeOccurenceStatistics : IPlaybackConfiguration
    {
        private readonly List<TypeOccurenceAggregator> _aggregators;
        private readonly Type[] _availableTypes;
        private readonly List<IInputStream> _inputs;
        private Dictionary<Type, long> _statistics;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOccurenceStatistics"/> class.
        /// </summary>
        /// <param name="availableTypes">Types for which the collection of statistics need to be enabled.</param>
        public TypeOccurenceStatistics(Type[] availableTypes)
        {
            _availableTypes = availableTypes;
            _aggregators = new List<TypeOccurenceAggregator>();
            _inputs = new List<IInputStream>();
        }

        public Dictionary<Type, long> Statistics
        {
            get { return _statistics; }
        }

        public void AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params Type[] typeMaps)
        {
            var mapInstances = new ITypeMap<TInput>[typeMaps.Length];
            for (int i = 0; i < typeMaps.Length; i++)
            {
                object o = Activator.CreateInstance(typeMaps[i]);
                if (o == null)
                    throw new Exception("Activator.CreateInstance failed for type " + typeMaps[i].Name);

                ITypeMap<TInput> mapInstance = o as ITypeMap<TInput>;
                if (mapInstance == null)
                    throw new Exception("The type " + typeMaps[i].FullName + " must implement one of these interfaces :"
                                        + typeof(ITypeMap<>).Name + ", "
                                        + typeof(IRootTypeMap<,>).Name + ", "
                                        + typeof(IPartitionableTypeMap<,>).Name);

                mapInstances[i] = mapInstance;
            }

            AddInput(createInput, mapInstances);
        }
        public void AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params ITypeMap<TInput>[] typeMaps)
        {
            var subject = new Subject<TInput>();

            foreach (var mapInstance in typeMaps)
            {
                Type mapInterface = mapInstance.GetType().GetTypeInfo().ImplementedInterfaces.FirstOrDefault(i => i.Name == typeof(IPartitionableTypeMap<,>).Name);
                if (mapInterface == null)
                    continue;
                Type aggregatorType =
                    typeof (TypeOccurenceAggregator<,>).MakeGenericType(mapInterface.GenericTypeArguments);
                object aggregatorInstance = Activator.CreateInstance(aggregatorType, mapInstance, _availableTypes);
                _aggregators.Add((TypeOccurenceAggregator) aggregatorInstance);

                subject.Subscribe((IObserver<TInput>) aggregatorInstance);
            }

            _inputs.Add(new InputStream<TInput>(createInput, subject));
        }

        public void Run()
        {
            foreach (IInputStream i in _inputs)
            {
                i.Start();
            }

            WaitHandle[] handles = (from a in _aggregators select a.Completed).ToArray();
            foreach (WaitHandle h in handles)
                h.WaitOne();

            // Merging collections that are usually small, so no worries about performance
            _statistics = new Dictionary<Type, long>();
            foreach (TypeOccurenceAggregator a in _aggregators)
            {
                if (a.Exception != null)
                    throw a.Exception;

                foreach (var pair in a.OccurenceStatistics)
                {
                    if (_statistics.ContainsKey(pair.Key))
                        _statistics[pair.Key] += pair.Value;
                    else
                        _statistics.Add(pair.Key, pair.Value);
                }
            }
        }

        private interface IInputStream
        {
            void Start();
        }

        private class InputStream<TInput> : IInputStream
        {
            private readonly IObservable<TInput> _input;
            private readonly IObserver<TInput> _observer;

            public InputStream(
                Expression<Func<IObservable<TInput>>> createInput,
                IObserver<TInput> observer)
            {
                _input = createInput.Compile()();
                _observer = observer;
            }

            public void Start()
            {
                _input.Subscribe(_observer);
            }
        }
    }
}