// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    /// <summary>
    /// Derive from BinaryTypeMap and implement the converters to use to deserialize the objects
    /// </summary>
    /// <typeparam name="T">The type that needs to be mapped to various output types</typeparam>
    /// <typeparam name="TKey">The type that is the Equality Comparer</typeparam>
    public abstract class BinaryTypeMap<T, TKey> : IPartitionableTypeMap<T, TKey>, IManifestLookup<TKey>
    {
        private readonly IEqualityComparer<TKey> comparer;

        protected readonly Dictionary<Type, KeyValuePair<TKey, Func<T, object>>> PayloadConverterCache =
            new Dictionary<Type, KeyValuePair<TKey, Func<T, object>>>();

        protected BinaryTypeMap(IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
        }

        public abstract Func<T, DateTimeOffset> TimeFunction
        {
            get;
        }

        public abstract TKey GetInputKey(T envelope);

        public Func<T, object> GetTransform(Type outputType)
        {
            if (outputType == null)
            {
                throw new ArgumentNullException("outputType");
            }

            KeyValuePair<TKey, Func<T, object>> value;

            if (this.PayloadConverterCache.TryGetValue(outputType, out value))
            {
                return value.Value;
            }

            return envelope => null;
        }

        public IEqualityComparer<TKey> Comparer
        {
            get { return this.comparer; }
        }

        public TKey GetTypeKey(Type outputType)
        {
            if (outputType == null)
            {
                throw new ArgumentNullException("outputType");
            }

            KeyValuePair<TKey, Func<T, object>> value;

            if (this.PayloadConverterCache.TryGetValue(outputType, out value))
            {
                return value.Key;
            }

            return default(TKey);
        }

        #region IManifestLookup
        public TKey LookupManifestId(Type type)
        {
            KeyValuePair<TKey, Func<T, object>> value;

            if (type != null && this.PayloadConverterCache.TryGetValue(type, out value))
            {
                return value.Key;
            }

            return default(TKey);
        }

        public Type TypeNameToType(string typeName)
        {
            foreach (var keyValuePair in this.PayloadConverterCache)
            {
                if (keyValuePair.Key.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    return keyValuePair.Key;
                }
            }

            return null;
        }

        public Type ManifestToType(string manifestId)
        {
            foreach (var keyValuePair in this.PayloadConverterCache)
            {
                if (keyValuePair.Value.Key.Equals(manifestId))
                {
                    return keyValuePair.Key;
                }
            }

            return null;
        }

        public IEnumerable<KeyValuePair<TKey, Type>> GetAllSupportedTypes()
        {
            var retVal = new List<KeyValuePair<TKey, Type>>(this.PayloadConverterCache.Count);

            foreach (var item in this.PayloadConverterCache)
            {
                retVal.Add(new KeyValuePair<TKey, Type>(item.Value.Key, item.Key));
            }

            return retVal;
        }
        #endregion
    }
}
