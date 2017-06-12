namespace Tx.Network.Syslogs
{
    using System;
    using System.Net;
    using System.Reactive;

    public class SyslogListener : BaseUdpReceiver<IEnvelope>
    {
        public SyslogListener(IPEndPoint listenEndPoint, uint concurrentReceivers)
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
                Syslog syslog;

                try
                {
                    syslog = SyslogParser.Parse(
                        upacket.UdpData, 
                        upacket.ReceivedTime, 
                        upacket.PacketHeader.SourceIpAddress.ToString());
                }
                catch (Exception)
                {
                    return false;
                }

                envelope = new SyslogEnvelope
                {
                    OccurrenceTime = packet.ReceivedTime,
                    ReceivedTime = packet.ReceivedTime,
                    Payload = upacket.UdpData.Array,
                    PayloadInstance = syslog,
                };
            }

            return isValid;
        }

        private sealed class SyslogEnvelope : IEnvelope
        {
            private static readonly string typeId = typeof(Syslog).GetTypeIdentifier();

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
                    return Tx.Network.Protocol.Syslog;
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
