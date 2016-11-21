namespace Tx.Network
{
    using System;
    using System.Net.Sockets;

    public static class Extensions
    {
        /// <summary>
        /// Creates UDP datagram based on the specified IP packet.
        /// </summary>
        /// <param name="ipPacket">A source IpPacket Object that contains a UDP datagram.</param>
        /// <returns>A new UdpDatagram instance.</returns>
        /// <exception cref="System.ArgumentNullException">ipPacket is null</exception>
        /// <exception cref="System.NotSupportedException">Only UDP packets are supported.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">ipPacket.PacketData is empty.</exception>
        public static UdpDatagram ToUdpDatagram(this IIpPacket ipPacket, bool reuseOriginalBuffer = true)
        {
            if (ipPacket == null)
            {
                throw new ArgumentNullException("ipPacket");
            }

            if (ipPacket.ProtocolType != ProtocolType.Udp)
            {
                throw new NotSupportedException("Only UDP packets are supported.");
            }

            if (ipPacket.PacketData.Count <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var packetData = ipPacket.PacketData.AsByteArraySegment();

            UdpDatagramHeader udpDatagramHeader = null;
            if (ipPacket.PacketData.Count > 8)
            {
                udpDatagramHeader = new UdpDatagramHeader(
                    packetData.Array.ReadNetOrderUShort(packetData.Offset),
                    packetData.Array.ReadNetOrderUShort(2 + packetData.Offset),
                    packetData.Array.ReadNetOrderUShort(4 + packetData.Offset),
                    packetData.Array.ReadNetOrderUShort(6 + packetData.Offset));
            }

            ArraySegment<byte> udpData;
            if (reuseOriginalBuffer)
            {
                udpData = new ArraySegment<byte>(
                    packetData.Array,
                    packetData.Offset + 8,
                    packetData.Count - 8);
            }
            else
            {
                var ipOptionsDataArray = new byte[packetData.Count - 8];
                Array.Copy(packetData.Array, packetData.Offset + 8, ipOptionsDataArray, 0, packetData.Count - 8);
                udpData = new ArraySegment<byte>(ipOptionsDataArray);
            }

            var udpDatagram = new UdpDatagram
            {
                PacketHeader = ipPacket.PacketHeader,
                UdpDatagramHeader = udpDatagramHeader,
                UdpData = udpData,
                ReceivedTime = ipPacket.ReceivedTime
            };

            return udpDatagram;
        }
    }
}