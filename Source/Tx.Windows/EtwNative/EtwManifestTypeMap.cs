// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    public class EtwManifestTypeMap : EtwTypeMap, IPartitionableTypeMap<EtwNativeEvent, ManifestEventPartitionKey>
    {
        private readonly ManifestEventPartitionKey.Comparer _comparer = new ManifestEventPartitionKey.Comparer();
        private readonly ManifestEventPartitionKey _key = new ManifestEventPartitionKey();

        public IEqualityComparer<ManifestEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ManifestEventPartitionKey GetInputKey(EtwNativeEvent evt)
        {
            // this avoids memory allocation per each event
            _key.EventId = evt.Id;
            _key.ProviderId = evt.ProviderId;
            _key.Version = evt.Version;
            return _key;
        }

        public ManifestEventPartitionKey GetTypeKey(Type outputType)
        {
            var eventAttribute = outputType.GetTypeInfo().GetCustomAttribute<ManifestEventAttribute>();
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