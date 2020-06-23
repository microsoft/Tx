// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tx.Windows
{
    /// <summary>
    /// This class implements lazy deserialization:
    ///     - It keeps a pointer to the ETW native structure
    ///     - If the caller accesses system fields like EventId, it returns them without copying
    ///     - If the caller asks for something more complex, such as Message the entire event is copied
    /// </summary>

    class EtwTdhEvent : IDictionary<string, object>
    {
        EtwTdhDeserializer _deserializer;

        EtwNativeEvent _nativeEvent;

        IDictionary<string, object> _materializedEvent;

        public EtwTdhEvent(EtwTdhDeserializer deserializer, EtwNativeEvent nativeEvent)
        {
            _deserializer = deserializer;
            _nativeEvent = nativeEvent;
        }

        /// <summary>
        /// Method that materializes the event br reading (deserializing) the ETW data
        /// Calling this second time is ignored as the event is already materialized
        /// </summary>
        void Materialize()
        {
            if (_materializedEvent != null)
            {
                return; // the event is already materialized
            }

            _materializedEvent = _deserializer.Deserialize(ref _nativeEvent);
        }

        static Dictionary<string, Func<EtwNativeEvent, object>> _systemFields = new Dictionary<string, Func<EtwNativeEvent, object>>
        {
            { "EventId", e=>e.Id },
            { "ProviderId", e=>e.ProviderId },
            { "Version", e=>e.Version },
            { "TimeCreated", e=>e.TimeStamp.UtcDateTime },
            { "ProcessId", e=>e.ProcessId },
            { "ThreadId", e=>e.ThreadId },
            { "ActivityId", e=>e.ActivityId }
        };

        public object this[string key]
        {
            get
            {
                Func<EtwNativeEvent, object> accessor = null;
                if (_systemFields.TryGetValue(key, out accessor))
                {
                    return accessor(_nativeEvent);
                }

                Materialize();
                return _materializedEvent[key];
            }

            set => throw new NotSupportedException();
        }

        public ICollection<string> Keys
        {
            get
            {
                Materialize();
                return _materializedEvent.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                if (_materializedEvent == null)
                {
                    Materialize();
                }

                return _materializedEvent.Values;
            }
        }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            if (_materializedEvent == null)
            {
                return _systemFields.ContainsKey(key);
            }

            return _materializedEvent.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _materializedEvent.GetEnumerator();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            if (_systemFields.TryGetValue(key, out Func<EtwNativeEvent, object> accessor))
            {
                value = accessor(_nativeEvent);
                return true;
            }

            Materialize();
            return _materializedEvent.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}