namespace Tx.Network
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    public static class NetworkTransformExtentions
    {
        ///// <summary>
        ///// Takes an IpPacket object and encodes the IP header for transmission on the network in a byte array. Includes computing checksums.
        ///// </summary>
        ///// <param name="Packet">Packet object for encoding.</param>
        ///// <returns>The IP header in network order byte array with checksums included.</returns>
        //public static byte[] PacketHeaderToWireBytes(this IpPacket Packet)
        //{
        //    using (var builderStream = new MemoryStream())
        //    {

        //        //ipVers  bits 0 to 3
        //        //InternetHeaderLength  bits 4 to 7
        //        var ipversion = Packet.IpVersion == NetworkInterfaceComponent.IPv4 ? 4 : 6;
        //        var byte0 = (byte)BitConverter.GetBytes((ipversion << 4) + Packet.InternetHeaderLength)[0];
        //        builderStream.WriteByte(byte0);

        //        //DscpValue 8 to 13
        //        //ExplicitCongestionNotice 14 to 15
        //        var byte1 = (byte)BitConverter.GetBytes(((int)Packet.DscpValue << 2) + Packet.ExplicitCongestionNotice)[0];
        //        builderStream.WriteByte(byte1);

        //        //IpPacketLength 16 to 31
        //        var byte23 = BitConverter.GetBytes(NetworkOrderUshort(Packet.IpPacketLength));
        //        builderStream.Write(byte23, 0, 2);

        //        //FragmentGroupId 32 to 47
        //        var byte45 = BitConverter.GetBytes(NetworkOrderUshort(Packet.FragmentGroupId));
        //        builderStream.Write(byte45, 0, 2);

        //        //IpHeaderFlags 48 to 50
        //        //FragmentOffset  51 to 63
        //        var byte67 = BitConverter.GetBytes(((int)Packet.IpHeaderFlags << 13) + Packet.FragmentOffset);
        //        builderStream.WriteByte(byte67[0]);
        //        builderStream.WriteByte(byte67[1]);

        //        //TimeToLive 64 to 71
        //        builderStream.WriteByte(Packet.TimeToLive);

        //        //ProtocolNumber 72 to 79
        //        builderStream.WriteByte(Packet.ProtocolNumber);

        //        //PacketHeaderChecksum  80 to 95
        //        builderStream.Write(BitConverter.GetBytes(0), 0, 2); //put all zeros in here and calculate it below.

        //        //SourceIpAddress 96 to 127
        //        builderStream.Write(Packet.SourceIpAddress.GetAddressBytes(), 0, 4);

        //        //DestinationIpAddress 128 to 160
        //        builderStream.Write(Packet.DestinationIpAddress.GetAddressBytes(), 0, 4);

        //        if (Packet.IpOptions != null)
        //        {
        //            builderStream.Write(Packet.IpOptions, 0, Packet.IpOptions.Length);
        //        }
        //        var headerBytes = builderStream.ToArray();
        //        var sum = GetInternetChecksum(headerBytes);
        //        Array.Copy(BitConverter.GetBytes(sum), 0, headerBytes, 10, 2);

        //        return headerBytes;
        //    }

        //}

        ///// <summary>
        ///// Takes a UDP Datagram object and encodes the UDP header for transmission on the network. Assumes UDP checksum is computed.
        ///// </summary>
        ///// <param name="input">Datagram object to encode.</param>
        ///// <returns>UDP header encoded in network order.</returns>
        internal static byte[] UdpHeaderToWireBytes(this IUdpDatagram input)
        {
            //udp header is 8 bytes
            using (var outPacket = new MemoryStream())
            {
                outPacket.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.SourcePort)), 0, 2);
                outPacket.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.DestinationPort)), 0, 2);
                outPacket.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.UdpLength)), 0, 2);
                outPacket.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.UdpCheckSum)), 0, 2); //should be zero if we haven't calculated it yet.
                return outPacket.ToArray();
            }
        }

        ///// <summary>
        ///// Takes an IpPacket object and encodes it for transmission on the network in a byte array. Includes computing checksums.
        ///// </summary>
        ///// <param name="packet">Packet object for encoding.</param>
        ///// <returns>Network order byte array with IP checksum included ready to send on a raw socket.</returns>
        //public static byte[] ToWireBytes(this IpPacket packet)
        //{
        //    using (var builderStream = new MemoryStream())
        //    {
        //        var header = PacketHeaderToWireBytes(packet);
        //        builderStream.Write(header, 0, header.Length);
        //        builderStream.Write(packet.PacketData, 0, packet.PacketData.Length);
        //        return builderStream.ToArray();
        //    }
        //}

        ///// <summary>
        ///// Takes an UDPDatagram object and encodes it for transmission on the network in a byte array. Includes computing checksums.
        ///// </summary>
        ///// <param name="input">UdpDatagram object for encoding.</param>
        ///// <returns>Network order byte array with IP and UDP checksums included ready to send on a raw socket.</returns>
        //public static byte[] ToWirebytes(this IUdpDatagram input)
        //{

        //    using (var builderStream = new MemoryStream())
        //    {
        //        var ipHeader = PacketHeaderToWireBytes(input);
        //        builderStream.Write(ipHeader, 0, ipHeader.Length);
        //        var udpCk = GetUdpCheckSum(input);
        //        input.UdpCheckSum = NetworkOrderUshort(udpCk);
        //        builderStream.Write(UdpHeaderToWireBytes(input), 0, 8); //bytes now in network order here
        //        builderStream.Write(input.UdpData, 0, input.UdpData.Length);
        //        return builderStream.ToArray();
        //    }
        //}

        ///// <summary>
        ///// Per RFC 768, compute a ones complement sum over the sum of the 16bit words in the byte array. If the array is too short a zero-pad byte is added.
        ///// </summary>
        ///// <param name="input">Byte array to compute the checksum on</param>
        ///// <returns>16 bit integer sum</returns>
        ///// <remarks>This is not a CRC but is the checksum used for IP headers, UDP datagrams or TCP segments.
        ///// Note that if the 16 bit integers on input are in network order, the answer will also be in network order.</remarks>
        internal static ushort GetInternetChecksum(byte[] input) //input should be in Host order
        {
            //make a copy because we may need to resize it; GC should get this on return
            byte[] _input = new byte[input.Length];
            Array.Copy(input, _input, input.Length);

            //zero pad
            if (_input.Length % 2 != 0)
            {
                Array.Resize(ref _input, _input.Length + 1);
                _input[_input.Length] = 0;
            }

            uint firstSum = 0;
            for (int i = 0; i < _input.Length; i += 2)
            {
                firstSum += (uint)_input.ReadUShort(i);
            }
            uint secondSum = 0;
            byte[] firstSumBytes = BitConverter.GetBytes(firstSum);

            for (int j = 0; j < 4; j += 2)
            {
                secondSum += (uint)firstSumBytes.ReadUShort(j);
            }
            if ((secondSum & 0xffff0000) > 0)
            {
                secondSum = secondSum & 0xffff;
                secondSum += 1;
            }
            ushort sum = (ushort)secondSum; //numeric overflow is handled by the above if block.
            return (ushort)(~sum & 0xFFFF);
        }

        /// <summary>
        /// From a UDP datagram object, creates the UDP pseudoheader that is used to compute the UDP checksum per RFC 768
        /// </summary>
        /// <param name="input">The datagram object to use in encoding</param>
        /// <returns>Byte array of the pseudo header in Network Order</returns>
        internal static byte[] UdpPseudoHeader(this IUdpDatagram input)
        {
            byte zeroes = 0;
            byte protocol = (byte)ProtocolType.Udp;

            using (var builder = new MemoryStream())
            {
                builder.Write(input.PacketHeader.SourceIpAddress.GetAddressBytes(), 0, 4);
                builder.Write(input.PacketHeader.DestinationIpAddress.GetAddressBytes(), 0, 4);
                builder.WriteByte(zeroes);
                builder.WriteByte(protocol);
                builder.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.UdpLength)), 0, 2);
                return builder.ToArray();
            }
        }

        /// <summary>
        /// Uses the UDP Pseudoheader, UDP Header, and UDP payload to compute the checksum used in UDP transmission, per RFC 768
        /// </summary>
        /// <param name="input">The UdpDatagram object to check</param>
        /// <returns>16bit integer sum in network order.</returns>
        public static ushort GetUdpCheckSum(this IUdpDatagram input)
        {
            using (var udpCk = new MemoryStream())
            {
                var udpPh = UdpPseudoHeader(input);
                udpCk.Write(udpPh, 0, udpPh.Length);

                udpCk.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.SourcePort)), 0, 2);
                udpCk.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.DestinationPort)), 0, 2);
                udpCk.Write(BitConverter.GetBytes(NetworkOrderUshort(input.UdpDatagramHeader.UdpLength)), 0, 2);
                udpCk.WriteByte(0);
                udpCk.WriteByte(0);

                var udpData = input.Data.AsByteArraySegment();

                udpCk.Write(udpData.Array, udpData.Offset, udpData.Count);

                return GetInternetChecksum(udpCk.ToArray());
            }
        }

        internal static ushort NetworkOrderUshort(ushort input)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)input);
        }
    }
}
