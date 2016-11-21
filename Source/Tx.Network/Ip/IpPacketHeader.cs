namespace Tx.Network
{
    using System.Net;

    /// <summary>
    /// Data model class that describes the header of IP packets.
    /// </summary>
    public class IpPacketHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IpPacketHeader"/> class.
        /// </summary>
        /// <param name="sourceIpAddress">Source IP Address, not assumed to be on the local host..</param>
        /// <param name="destinationIpAddress">Destination IP Address.</param>
        /// <param name="isIp6">if set to <c>true</c> IP version is 6 otherwise 4.</param>
        /// <param name="internetHeaderLength">Length of the internet header.</param>
        /// <param name="dscpValue">DSCP value to encode.</param>
        /// <param name="explicitCongestionNotice">ECN value to encode.</param>
        /// <param name="ipPacketLength">Length of the complete packet.</param>
        /// <param name="fragmentGroupId">Fragement identifier, could be 0..</param>
        /// <param name="ipHeaderFlags">The IP header flags.</param>
        /// <param name="fragmentOffset">The fragment offset, could be 0.</param>
        /// <param name="timeToLive">Internet TTL.</param>
        /// <param name="packetHeaderChecksum">The packet header checksum.</param>
        public IpPacketHeader(
            IPAddress sourceIpAddress,
            IPAddress destinationIpAddress,
            bool isIp6,
            byte internetHeaderLength,
            byte dscpValue,
            byte explicitCongestionNotice,
            ushort ipPacketLength,
            ushort fragmentGroupId,
            byte ipHeaderFlags,
            ushort fragmentOffset,
            byte timeToLive,
            ushort packetHeaderChecksum)
        {
            this.SourceIpAddress = sourceIpAddress;
            this.DestinationIpAddress = destinationIpAddress;
            this.IsIp6 = isIp6;
            this.InternetHeaderLength = internetHeaderLength;
            this.DscpValue = dscpValue;
            this.ExplicitCongestionNotice = explicitCongestionNotice;
            this.IpPacketLength = ipPacketLength;
            this.FragmentGroupId = fragmentGroupId;
            this.IpHeaderFlags = ipHeaderFlags;
            this.FragmentOffset = fragmentOffset;
            this.TimeToLive = timeToLive;
            this.PacketHeaderChecksum = packetHeaderChecksum;
        }

        /// <summary>
        /// Source IP Address, not assumed to be on the local host.
        /// </summary>
        public readonly IPAddress SourceIpAddress;

        /// <summary>
        /// Destination IP Address.
        /// </summary>
        public readonly IPAddress DestinationIpAddress;

        /// <summary>
        /// The is ip6
        /// </summary>
        public readonly bool IsIp6;

        /// <summary>
        /// The internet header length
        /// </summary>
        public readonly byte InternetHeaderLength;

        /// <summary>
        /// DSCP value to encode.
        /// </summary>
        public readonly byte DscpValue;

        /// <summary>
        /// ECN value to encode.
        /// </summary>
        public readonly byte ExplicitCongestionNotice;

        /// <summary>
        /// Length of the complete packet.
        /// </summary>
        public readonly ushort IpPacketLength;

        /// <summary>
        /// Fragement identifier, could be 0.
        /// </summary>
        public readonly ushort FragmentGroupId;

        /// <summary>
        /// The IP header flags.
        /// </summary>
        public readonly byte IpHeaderFlags;

        /// <summary>
        /// The fragment offset, could be 0.
        /// </summary>
        public readonly ushort FragmentOffset;

        /// <summary>
        /// Internet TTL.
        /// </summary>
        public readonly byte TimeToLive;

        /// <summary>
        /// The packet header checksum.
        /// </summary>
        public readonly ushort PacketHeaderChecksum;
    }
}