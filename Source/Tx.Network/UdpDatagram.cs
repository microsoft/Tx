namespace Tx.Network
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Class to represent UDP datagram which contains the UDP headers and Data.
    /// </summary>
    public class UdpDatagram : IpPacket
    {
        #region Public Members
        /// <summary>
        /// Gets the source port.
        /// </summary>
        /// <value>
        /// The source port.
        /// </value>
        public ushort SourcePort { get; internal set; }

        /// <summary>
        /// Gets the destination port.
        /// </summary>
        /// <value>
        /// The destination port.
        /// </value>
        public ushort DestinationPort { get; internal set; }

        /// <summary>
        /// Gets the length of the UDP.
        /// </summary>
        /// <value>
        /// The length of the UDP.
        /// </value>
        public ushort UdpLength { get; internal set; }

        /// <summary>
        /// Gets the UDP check sum.
        /// </summary>
        /// <value>
        /// The UDP check sum.
        /// </value>
        public ushort UdpCheckSum { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this instance is UDP.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is UDP; otherwise, <c>false</c>.
        /// </value>
        public bool IsUdp { get; private set; }

        /// <summary>
        /// Gets the UDP data.
        /// </summary>
        /// <value>
        /// The UDP data.
        /// </value>
        public byte[] UdpData { get; private set; }

        /// <summary>
        /// Object parsed from the <seealso cref="UdpData"/>.
        /// </summary>
        /// <example><see cref="Tx.Network.Snmp.SnmpDatagram"/></example>
        public object TransportObject { get; internal set; }
        #endregion

        #region Constructors
        public UdpDatagram() : base()
        {
            UdpData = new byte[64];
        }
        /// <summary>
        /// Decodes a UdpDatagram Datagram
        /// </summary>
        public UdpDatagram(byte[] Buffer) : this(new IpPacket(Buffer)) { }

        /// <summary>
        /// Decodes a UdpDatagram Datagram
        /// </summary>
        public UdpDatagram(Stream stream) : this(new IpPacket(ConvertStreamToArray(stream))) { }

        /// <summary>
        /// Decodes a UdpDatagram Datagram
        /// </summary>
        public UdpDatagram(IpPacket ReceivedPacket)
            : base(ReceivedPacket)
        {

            if (Protocol != ProtocolType.Udp) throw new ArgumentException("Received Packet is not UDP");

            IsUdp = true;
            UdpData = new byte[PacketData.Length - 8];
            Array.Copy(PacketData, 8, UdpData, 0, PacketData.Length - 8);

            if (PacketData.Length > 8)
            {
                SourcePort = PacketData.ReadNetOrderUShort(0);
                DestinationPort = PacketData.ReadNetOrderUShort(2);
                UdpLength = PacketData.ReadNetOrderUShort(4);
                UdpCheckSum = PacketData.ReadNetOrderUShort(6);
            }
        }
        /// <summary>
        /// Used to create a primitive packet or datagram for encoding to the network.
        /// </summary>
        /// <param name="Packet">A source IpPacket Object that contains a UDP datagram</param>
        /// <param name="SourcePort">Source port for the datagram.</param>
        /// <param name="DestinationPort">Destination port for the datagram</param>
        /// <param name="UdpLength">The length of the UDP datagram including the UDP header</param>
        /// <param name="UdpData">The data following the UDP header. If this is null the Packet object's packet data is used.</param>
        /// <remarks>The IP Header and UDP Checksum is set by calling the ToWirebytes method to encode the packet to the wire.</remarks>
        public UdpDatagram(IpPacket Packet, ushort SourcePort, ushort DestinationPort, ushort UdpLength, byte[] UdpData)
                : base(Packet)
        {
            if (Packet.Protocol != ProtocolType.Udp) throw new InvalidDataException("Input Packet must be of protocol type UDP");

            this.SourcePort = SourcePort;
            this.DestinationPort = DestinationPort;
            this.UdpLength = UdpLength;
            this.UdpCheckSum = 0;
            this.UdpData = new byte[UdpLength - 8];
            if (UdpData == null)
            {
                Array.Copy(Packet.PacketData, Packet.InternetHeaderLength * 4 + 8, this.UdpData, 0, UdpLength - 8);
            }
            else {
                Array.Copy(UdpData, this.UdpData, UdpLength - 8);
            }
            IsUdp = true;

        }

        #endregion

        #region Methods
        /// <summary>
        /// Provides a string representation of the UdpDatagram IpPacket.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return string.Join(Environment.NewLine, base.ToString(),
                string.Format("Source Port: {0} Destination Port: {1} UDP Length: {2} UDP Checksum: {3}", SourcePort, DestinationPort, UdpLength, UdpCheckSum),
                Environment.NewLine
            );
        }
        #endregion

        #region PrivateMethods
        /// <summary>
        /// Converts the stream to array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>byte array</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">stream;Input stream is empty or null</exception>
        private static byte[] ConvertStreamToArray(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentOutOfRangeException("stream", "Input stream is empty or null");
            }

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        #endregion
    }
}


