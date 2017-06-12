// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    public class EtwClassicTypeMap : EtwTypeMap, IPartitionableTypeMap<EtwNativeEvent, ClassicEventPartitionKey>
    {
        private readonly ClassicEventPartitionKey.Comparer _comparer = new ClassicEventPartitionKey.Comparer();
        private readonly ClassicEventPartitionKey _key = new ClassicEventPartitionKey();

        public IEqualityComparer<ClassicEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ClassicEventPartitionKey GetInputKey(EtwNativeEvent evt)
        {
            // this avoids memory allocation per each event
            _key.Opcode = evt.Opcode;
            _key.EventGuid = evt.ProviderId;
            _key.Version = evt.Version;
            return _key;
        }

        public ClassicEventPartitionKey GetTypeKey(Type outputType)
        {
            var eventAttribute = outputType.GetTypeInfo().GetCustomAttribute<ClassicEventAttribute>();
            if (eventAttribute == null)
                return null;

            return new ClassicEventPartitionKey
                {
                    EventGuid = eventAttribute.EventGuid,
                    Opcode = (byte) eventAttribute.Opcode,
                    Version = eventAttribute.Version
                };
        }
    }
}