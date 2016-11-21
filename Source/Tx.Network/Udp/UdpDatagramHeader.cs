namespace Tx.Network
{
    /// <summary>
    /// Data model class that describes the header of UDP datagrams.
    /// </summary>
    public class UdpDatagramHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpDatagramHeader"/> class.
        /// </summary>
        /// <param name="sourcePort">The source port.</param>
        /// <param name="destinationPort">The destination port.</param>
        /// <param name="udpLength">The length of the UDP datagram including the UDP header.</param>
        /// <param name="udpCheckSum">The UDP check sum.</param>
        public UdpDatagramHeader(ushort sourcePort, ushort destinationPort, ushort udpLength, ushort udpCheckSum)
        {
            this.SourcePort = sourcePort;
            this.DestinationPort = destinationPort;
            this.UdpLength = udpLength;
            this.UdpCheckSum = udpCheckSum;
        }

        /// <summary>
        /// The source port.
        /// </summary>
        public readonly ushort SourcePort;

        /// <summary>
        /// The destination port.
        /// </summary>
        public readonly ushort DestinationPort;

        /// <summary>
        /// The length of the UDP datagram including the UDP header.
        /// </summary>
        public readonly ushort UdpLength;

        /// <summary>
        /// The UDP check sum.
        /// </summary>
        public readonly ushort UdpCheckSum;
    }
}