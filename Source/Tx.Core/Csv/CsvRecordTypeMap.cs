// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Collections.Generic;

    public abstract class CsvRecordTypeMap : IPartitionableTypeMap<Record, bool>
    {
        private readonly IEqualityComparer<bool> _comparer = EqualityComparer<bool>.Default;

        private readonly Dictionary<Type, KeyValuePair<Record, Func<Record, object>>> _payloadConverterCache =
            new Dictionary<Type, KeyValuePair<Record, Func<Record, object>>>();


        /// <summary>
        /// Initializes a new instance of the <see cref="CsvRecordTypeMap"/> class.
        /// </summary>
        protected CsvRecordTypeMap()
        {
            this._payloadConverterCache.Add(
                typeof(string[]),
                new KeyValuePair<Record, Func<Record, object>>(new Record(), record => record.Items));
            this._payloadConverterCache.Add(
                typeof(Record),
                new KeyValuePair<Record, Func<Record, object>>(new Record(), record => record));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvRecordTypeMap"/> class.
        /// </summary>
        protected CsvRecordTypeMap(Type type, Func<Record, object> transform) : this()
        {
            this._payloadConverterCache.Add(
                type,
                new KeyValuePair<Record, Func<Record, object>>(new Record(), transform));
        }

        public abstract Func<Record, object> Transform { get; set; }

        public abstract Func<Record, DateTimeOffset> TimeFunction { get; }

        public bool GetInputKey(Record envelope)
        {
            return true;
        }

        public Func<Record, object> GetTransform(Type outputType)
        {
            if (outputType == null)
            {
                throw new ArgumentNullException(nameof(outputType));
            }

            KeyValuePair<Record, Func<Record, object>> value;

            if (this._payloadConverterCache.TryGetValue(outputType, out value))
            {
                return value.Value;
            }

            return envelope => null;
        }

        public IEqualityComparer<bool> Comparer
        {
            get { return this._comparer; }
        }

        public bool GetTypeKey(Type outputType)
        {
            if (outputType == null)
            {
                return false;
            }

            KeyValuePair<Record, Func<Record, object>> value;

            if (this._payloadConverterCache.TryGetValue(outputType, out value))
            {
                return true;
            }

            return false;
        }
    }
}
