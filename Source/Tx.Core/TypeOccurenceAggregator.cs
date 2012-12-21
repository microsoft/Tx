using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive
{
    abstract class TypeOccurenceAggregator
    {
        protected ManualResetEvent _completed = new ManualResetEvent(false);
        protected Exception _exception; 

        public WaitHandle Completed { get { return _completed; } }
        public Exception Exception { get { return _exception; } }

        public abstract Dictionary<Type, long> OccurenceStatistics { get; }
    }

    class TypeOccurenceAggregator<TInput, TKey> : TypeOccurenceAggregator, IObserver<TInput>
    {
        Dictionary<TKey, Type> _types;
        IPartitionableTypeMap<TInput, TKey> _typeMap;
        Dictionary<TKey, OccurenceRecord> _occurences;
        object _gate = new object();

        public TypeOccurenceAggregator(
            IPartitionableTypeMap<TInput, TKey> typeMap,
            Type[] AvailableTypes)
        {
            _typeMap = typeMap;
            _occurences = new Dictionary<TKey, OccurenceRecord>(_typeMap.Comparer);

            // The usage pattern is to pass all types in the Appdomain.
            // Only small subset will be recognized by this type-map
            // Let's build index for quick lookup
            _types = new Dictionary<TKey,Type>(_typeMap.Comparer);

            foreach(Type t in AvailableTypes)
            {
                TKey key = typeMap.GetTypeKey(t);

                if (key != null && !key.Equals(default(TKey)))
                {
                    if (!_types.ContainsKey(key))
                        _types.Add(key, t);
                }
            }
        }

        public void OnCompleted()
        {
            _completed.Set();
        }

        public void OnError(Exception error)
        {
            _exception = error;
            _completed.Set();
        }

        public void OnNext(TInput value)
        {
            TKey key = _typeMap.GetInputKey(value);
            Type type;
            if (!_types.TryGetValue(key, out type))
                return;

            OccurenceRecord record;
            if (!_occurences.TryGetValue(key, out record))
            {
                _occurences.Add(key, new OccurenceRecord
                {
                    Occurences = 1,
                    Type = type
                });
            }
            else
            {
                record.Occurences++;
            }
        }

        public override Dictionary<Type, long> OccurenceStatistics
        {
            get
            {
                Dictionary<Type, long> result = new Dictionary<Type, long>();
                foreach (OccurenceRecord record in _occurences.Values)
                {
                    result.Add(record.Type, record.Occurences);
                }

                return result;
            }
        }

        class OccurenceRecord
        {
            public long Occurences;
            public Type Type;
        }
    }
}
