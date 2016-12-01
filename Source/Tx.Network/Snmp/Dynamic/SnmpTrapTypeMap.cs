namespace Tx.Network.Snmp.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    using Tx.Network;

    public class SnmpTrapTypeMap : IPartitionableTypeMap<IEnvelope, ObjectIdentifier>
    {
        private readonly TrapTypeMap trapTypeMap = new TrapTypeMap();

        private SnmpDatagram snmpDatagram;

        public IEqualityComparer<ObjectIdentifier> Comparer => this.trapTypeMap.Comparer;

        public ObjectIdentifier GetTypeKey(Type outputType)
        {
            return this.trapTypeMap.GetTypeKey(outputType);
        }

        public Func<IEnvelope, object> GetTransform(Type outputType)
        {
            var transform = this.trapTypeMap.GetTransform(outputType);

            return transform != null ? _ => transform(this.snmpDatagram) : (Func<IEnvelope, object>)null;
        }

        public Func<IEnvelope, DateTimeOffset> TimeFunction => GetTime;

        public ObjectIdentifier GetInputKey(IEnvelope envelope)
        {
            this.snmpDatagram = envelope.PayloadInstance as SnmpDatagram;

            if (this.snmpDatagram == null && string.Equals(envelope.Protocol, Protocol.SnmpTrap, StringComparison.OrdinalIgnoreCase))
            {
                this.snmpDatagram = envelope.Payload.AsByteArraySegment()
                    .ToSnmpDatagram(envelope.ReceivedTime, "0.0.0.0");
            }

            if (this.snmpDatagram == null)
            {
                return default(ObjectIdentifier);
            }

            return this.trapTypeMap.GetInputKey(this.snmpDatagram);
        }

        private static DateTimeOffset GetTime(IEnvelope envelope)
        {
            var time = envelope.ReceivedTime;

            return time;
        }
    }
}
