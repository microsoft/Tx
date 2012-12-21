using System;
using System.Collections.Generic;
using System.Reactive;

namespace Tx.Windows
{
    public class EtwManifestTypeMap : EtwTypeMap, IPartitionableTypeMap<EtwNativeEvent, ManifestEventPartitionKey>
    {
        ManifestEventPartitionKey.Comparer _comparer = new ManifestEventPartitionKey.Comparer();
        ManifestEventPartitionKey _key = new ManifestEventPartitionKey();

        public IEqualityComparer<ManifestEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ManifestEventPartitionKey GetInputKey(EtwNativeEvent evt)
        {
            // For deserializer, it is useful to avoid allocation
            // For Type Statistics, we want to keep an instance of the key in the dictionary (so don't overwrite)
            // I will return to this after measuring performance
            //
            //_key.EventId = evt.Id; 
            //_key.ProviderId = evt.ProviderId;
            //_key.Version = evt.Version;
            //return _key;

            return new ManifestEventPartitionKey
            {
                EventId = evt.Id,
                ProviderId = evt.ProviderId,
                Version = evt.Version
            };
        }

        public ManifestEventPartitionKey GetTypeKey(Type outputType) 
        {
            var eventAttribute = outputType.GetAttribute<ManifestEventAttribute>();
            if (eventAttribute == null)
                return null;

            return new ManifestEventPartitionKey
            {
                ProviderId = eventAttribute.ProviderGuid,
                EventId = (ushort)eventAttribute.EventId,
                Version = eventAttribute.Version
            };
        }
    }
}
