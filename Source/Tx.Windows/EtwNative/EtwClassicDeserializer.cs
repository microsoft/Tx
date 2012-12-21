using System;
using System.Collections.Generic;
using System.Reactive;

namespace Tx.Windows
{
    public class EtwClassicTypeMap : EtwTypeMap, IPartitionableTypeMap<EtwNativeEvent, ClassicEventPartitionKey>
    {
        ClassicEventPartitionKey.Comparer _comparer = new ClassicEventPartitionKey.Comparer();
        ClassicEventPartitionKey _key = new ClassicEventPartitionKey();

        public IEqualityComparer<ClassicEventPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public ClassicEventPartitionKey GetInputKey(EtwNativeEvent evt)
        {
            // See comments in ManifestEventTypeMap
            //_key.Opcode = evt.Opcode;
            //_key.EventGuid = evt.ProviderId;
            //_key.Version = evt.Version;
            //return _key;

            return new ClassicEventPartitionKey
            {
                Opcode = evt.Opcode,
                EventGuid = evt.ProviderId,
                Version = evt.Version
            };
        }

        public ClassicEventPartitionKey GetTypeKey(Type outputType)
        {
            var eventAttribute = outputType.GetAttribute<ClassicEventAttribute>();
            if (eventAttribute == null)
                return null;

            return new ClassicEventPartitionKey
            {
                EventGuid = eventAttribute.EventGuid,
                Opcode = (byte)eventAttribute.Opcode,
                Version = eventAttribute.Version
            };
        }
    }
}
