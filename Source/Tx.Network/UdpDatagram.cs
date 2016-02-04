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


