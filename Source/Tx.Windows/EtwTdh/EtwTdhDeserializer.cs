// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Tx.Windows
{
    public class EtwTdhDeserializer
    {
        public ulong SequenceNumber { get; private set; } // For comparing the data with Message Analyzer
        Dictionary<Guid, Dictionary<uint, EtwTdhEventInfo>> _cache = new Dictionary<Guid, Dictionary<uint, EtwTdhEventInfo>>();

        public IDictionary<string, object> Deserialize(ref EtwNativeEvent e)
        {
            SequenceNumber++;

            Dictionary<uint, EtwTdhEventInfo> providerInfo = null;
            if (!_cache.TryGetValue(e.ProviderId, out providerInfo))
            {
                providerInfo = new Dictionary<uint, EtwTdhEventInfo>();
                _cache.Add(e.ProviderId, providerInfo);
            }

            EtwTdhEventInfo info = null;
            if (!providerInfo.TryGetValue(e.Id, out info))
            {
                info = new EtwTdhEventInfo(ref e);
                providerInfo.Add(e.Id, info);
            }

            var result = info.Deserialize(ref e);

            return result;
        }
    }
}
