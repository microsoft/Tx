namespace Tx.Network.Snmp.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    using Tx.Network;
/*
    public class SnmpTrapTypeMap : IPartitionableTypeMap<IEnvelope, ObjectIdentifier>
    {
        private readonly TrapTypeMap trapTypeMap = new TrapTypeMap();

        private SnmpDatagram udpDatagram;

        public IEqualityComparer<ObjectIdentifier> Comparer
        {
            get { return this.trapTypeMap.Comparer; }
        }

        public ObjectIdentifier GetTypeKey(Type outputType)
        {
            return this.trapTypeMap.GetTypeKey(outputType);
        }

        public Func<IEnvelope, object> GetTransform(Type outputType)
        {
            var transform = this.trapTypeMap.GetTransform(outputType);

            return transform != null ? _ => transform(this.udpDatagram) : (Func<IEnvelope, object>)null;
        }

        public Func<IEnvelope, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetTime;
            }
        }

        public ObjectIdentifier GetInputKey(IEnvelope envelope)
        {
            this.udpDatagram = envelope.PayloadInstance as SnmpDatagram;

            if (this.udpDatagram == null && string.Equals(envelope.Protocol, Protocol.SnmpTrap, StringComparison.OrdinalIgnoreCase))
            {
                this.udpDatagram = envelope.Payload.AsByteArraySegment()
                    .ToSnmpDatagram(envelope.ReceivedTime, "");
            }

            if (this.udpDatagram == null)
            {
                return default(ObjectIdentifier);
            }

            return this.trapTypeMap.GetInputKey(this.udpDatagram);
        }

        private static DateTimeOffset GetTime(IEnvelope envelope)
        {
            var time = envelope.ReceivedTime;

            return time;
        }
    }
*/
}
