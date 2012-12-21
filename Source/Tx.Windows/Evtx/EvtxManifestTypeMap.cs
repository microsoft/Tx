using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Reactive;

namespace Tx.Windows
{
    public class EvtxManifestTypeMap : EvtxTypeMap, IPartitionableTypeMap<EventRecord, ManifestEventPartitionKey>
    {
        ManifestEventPartitionKey.Comparer _comparer = new ManifestEventPartitionKey.Comparer();
        ManifestEventPartitionKey _key = new ManifestEventPartitionKey();

        public IEqualityComparer<ManifestEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ManifestEventPartitionKey GetInputKey(EventRecord evt)
        {
            return new ManifestEventPartitionKey
            {
                EventId = (ushort)evt.Id,
                ProviderId = evt.ProviderId.Value,
                Version = evt.Version.Value
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
