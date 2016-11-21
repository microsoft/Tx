namespace Tx.Network
{
    using System.Net;

    public class UdpReceiver : BaseUdpReceiver<IUdpDatagram>
    {
        public UdpReceiver(string ipAddress, ushort port = 514, uint concurrentReceivers = 10)
            : this(new IPEndPoint(IPAddress.Parse(ipAddress), port), concurrentReceivers)
        {            
        }

        public UdpReceiver(IPEndPoint listenEndPoint, uint concurrentReceivers = 10)
            : base(listenEndPoint, concurrentReceivers)
        {
        }

        protected override bool TryParse(IpPacket packet, out IUdpDatagram result)
        {
            result = null;

            if (!packet.PacketHeader.DestinationIpAddress.Equals(this.ListenEndPoint.Address))
            {
                return false;
            }

            var upacket = packet.ToUdpDatagram();

            var isValid = upacket.UdpDatagramHeader.DestinationPort == this.ListenEndPoint.Port;

            if (isValid)
            {
                result = upacket;
            }

            return isValid;
        }
    }
}
