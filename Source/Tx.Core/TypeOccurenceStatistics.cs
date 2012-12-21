using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading;
using System.Reactive.Subjects;

namespace System.Reactive
{
    public class TypeOccurenceStatistics : IPlaybackConfiguration
    {
        Type[] _availableTypes;
        List<TypeOccurenceAggregator> _aggregators;
        List<InputStream> _inputs;
        Dictionary<Type, long> _statistics;

        public TypeOccurenceStatistics(Type[] availableTypes)
        {
            _availableTypes = availableTypes;
            _aggregators = new List<TypeOccurenceAggregator>();
            _inputs = new List<InputStream>();
        }

        public void AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput, 
            params Type[] typeMaps)
        {
            Subject<TInput> subject = new Subject<TInput>();

            foreach (Type mapType in typeMaps)
            {
                var mapInstance = Activator.CreateInstance(mapType);
                var mapInterface = mapType.GetInterface(typeof(IPartitionableTypeMap<,>).Name);
                if (mapInterface == null)
                    continue;
                var aggregatorType = typeof(TypeOccurenceAggregator<,>).MakeGenericType(mapInterface.GetGenericArguments());
                var aggregatorInstance = Activator.CreateInstance(aggregatorType, mapInstance, _availableTypes);
                _aggregators.Add((TypeOccurenceAggregator)aggregatorInstance);

                subject.Subscribe((IObserver<TInput>)aggregatorInstance);
            }

            _inputs.Add(new InputStream<TInput>(this, createInput, subject));
        }

        public void Run()
        {
            foreach (InputStream i in _inputs)
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

                foreach(KeyValuePair<Type,long> pair in a.OccurenceStatistics)
                {
                    if (_statistics.ContainsKey(pair.Key))
                        _statistics[pair.Key] += pair.Value;
                    else
                        _statistics.Add(pair.Key, pair.Value);
                }
            }
        }

        public Dictionary<Type, long> Statistics
        {
            get { return _statistics; }
        }

        interface InputStream
        {
            void Start();
        }

        class InputStream<TInput> : InputStream
        {
            TypeOccurenceStatistics _parent;
            IObserver<TInput> _observer;
            IObservable<TInput> _input;

            public InputStream(
                TypeOccurenceStatistics parent,                 
                Expression<Func<IObservable<TInput>>> createInput, 
                IObserver<TInput> observer)
            {
                _parent = parent;
                _input = createInput.Compile()();
                _observer = observer;
            }

            public void Start()
            {
                _input.Subscribe(_observer);
            }
        }

        class CountHolder
        {
            public long Count { get; set; }
        }
    }
}
