// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Reactive;

namespace Tx.Windows
{
    public class EvtxManifestTypeMap : EvtxTypeMap, IPartitionableTypeMap<EventRecord, ManifestEventPartitionKey>
    {
        private readonly ManifestEventPartitionKey.Comparer _comparer = new ManifestEventPartitionKey.Comparer();

        public IEqualityComparer<ManifestEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ManifestEventPartitionKey GetInputKey(EventRecord evt)
        {
            return new ManifestEventPartitionKey
                {
                    EventId = (ushort) evt.Id,
                    ProviderId = evt.ProviderId.HasValue ? evt.ProviderId.Value : Guid.Empty, // looks like in evtx files we can also have name instead of Guid?
                    Version = evt.Version.HasValue ? evt.Version.Value : (byte) 0
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
                    EventId = (ushort) eventAttribute.EventId,
                    Version = eventAttribute.Version
                };
        }
    }
}