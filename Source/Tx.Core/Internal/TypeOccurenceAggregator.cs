// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Reactive
{
    internal abstract class TypeOccurenceAggregator
    {
        protected ManualResetEvent _completed = new ManualResetEvent(false);
        protected Exception _exception;

        public WaitHandle Completed
        {
            get { return _completed; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public abstract Dictionary<Type, long> OccurenceStatistics { get; }
    }

    internal class TypeOccurenceAggregator<TInput, TKey> : TypeOccurenceAggregator, IObserver<TInput>
    {
        private readonly Dictionary<TKey, OccurenceRecord> _occurences;
        private readonly IPartitionableTypeMap<TInput, TKey> _typeMap;
        private readonly Dictionary<TKey, Type> _types;

        public TypeOccurenceAggregator(
            IPartitionableTypeMap<TInput, TKey> typeMap,
            IEnumerable<Type> availableTypes)
        {
            _typeMap = typeMap;
            _occurences = new Dictionary<TKey, OccurenceRecord>(_typeMap.Comparer);

            // The usage pattern is to pass all types in the Appdomain.
            // Only small subset will be recognized by this type-map
            // Let's build index for quick lookup
            _types = new Dictionary<TKey, Type>(_typeMap.Comparer);

            foreach (Type t in availableTypes)
            {
                TKey key = typeMap.GetTypeKey(t);

                if (key != null && !key.Equals(default(TKey)))
                {
                    if (!_types.ContainsKey(key))
                        _types.Add(key, t);
                }
            }
        }

        public override Dictionary<Type, long> OccurenceStatistics
        {
            get
            {
                return _occurences.Values.ToDictionary(record => record.Type, record => record.Occurences);
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

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
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

        private class OccurenceRecord
        {
            public long Occurences;
            public Type Type;
        }
    }
}