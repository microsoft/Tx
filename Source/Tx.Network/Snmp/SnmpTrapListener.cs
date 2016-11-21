namespace Tx.Network.Snmp
{
    using System;
    using System.Net;
    using System.Reactive;

    public class SnmpTrapListener : BaseUdpReceiver<IEnvelope>
    {
        public SnmpTrapListener(IPEndPoint listenEndPoint, uint concurrentReceivers)
            : base(listenEndPoint, concurrentReceivers)
        {
        }

        protected override bool TryParse(IpPacket packet, out IEnvelope envelope)
        {
            envelope = null;

            if (!packet.PacketHeader.DestinationIpAddress.Equals(this.ListenEndPoint.Address))
            {
                return false;
            }

            var upacket = packet.ToUdpDatagram(false);

            var isValid = upacket.UdpDatagramHeader.DestinationPort == this.ListenEndPoint.Port;

            if (isValid)
            {
                SnmpDatagram datagram;
                if (upacket.TryParseSnmpDatagram(out datagram))
                {
                    isValid = datagram.Header.Version == SnmpVersion.V1 || datagram.Header.Version == SnmpVersion.V2C;

                    if (isValid)
                    {
                        envelope = new SnmpTrapEnvelope
                        {
                            OccurrenceTime = packet.ReceivedTime,
                            ReceivedTime = packet.ReceivedTime,
                            Payload = upacket.UdpData.Array,
                            PayloadInstance = new SnmpDatagram(
                                packet.ReceivedTime,
                                packet.PacketHeader.SourceIpAddress.ToString(),
                                datagram.Header,
                                datagram.VarBinds),
                        };
                    }
                }
            }

            return isValid;
        }

        private sealed class SnmpTrapEnvelope : IEnvelope
        {
            private static readonly string typeId = typeof(SnmpDatagram).GetTypeIdentifier();

            public DateTimeOffset OccurrenceTime { get; internal set; }

            public DateTimeOffset ReceivedTime { get; internal set; }

            public byte[] Payload { get; internal set; }

            public object PayloadInstance { get; internal set; }

            public string Source
            {
                get
                {
                    return string.Empty;
                }
            }

            public string Protocol
            {
                get
                {
                    return Tx.Network.Protocol.SnmpTrap;
                }
            }

            public string TypeId
            {
                get
                {
                    return typeId;
                }
            }
        }
    }
}
