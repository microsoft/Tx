namespace Tx.Network
{
    using System;
    using System.Net;
    using System.Reactive;

    public class UdpEnvelopeReceiver : BaseUdpReceiver<IEnvelope>
    {
        public UdpEnvelopeReceiver(IPEndPoint listenEndPoint, uint concurrentReceivers = 10)
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

            var upacket = packet.ToUdpDatagram();

            var isValid = upacket.UdpDatagramHeader.DestinationPort == this.ListenEndPoint.Port;

            if (isValid)
            {
                envelope = new UdpEnvelope
                {
                    OccurrenceTime = packet.ReceivedTime,
                    ReceivedTime = packet.ReceivedTime,
                    Payload = packet.PacketData.Array,
                    PayloadInstance = upacket,
                };
            }

            return isValid;
        }

        private sealed class UdpEnvelope : IEnvelope
        {
            private static readonly string typeId = typeof(UdpDatagram).GetTypeIdentifier();

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
                    return Tx.Network.Protocol.UdpDatagram;
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