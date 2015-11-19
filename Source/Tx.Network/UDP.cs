
namespace Tx.Network
{
    using System;
    using System.Net.Sockets;
    public class UdpDatagram : IpPacket
    {
        #region Public Members
        public ushort SourcePort { get; internal set; }
        public ushort DestinationPort { get; internal set; }
        public ushort UdpLength { get; internal set; }
        public ushort UdpCheckSum { get; internal set; }
        public bool IsUdp { get; private set; }
        public byte[] UdpData { get; private set; }
        #endregion

        #region Constructors
        public UdpDatagram()
        {
            IsUdp = false;
        }
        /// <summary>
        /// Decodes a UdpDatagram Datagram
        /// </summary>
        public UdpDatagram(byte[] Buffer) : this(new IpPacket(Buffer)) { }

        /// <summary>
        /// Decodes a UdpDatagram Datagram
        /// </summary>
        public UdpDatagram(IpPacket ReceivedPacket) : base(ReceivedPacket)
        {
            if (Protocol == ProtocolType.Udp) IsUdp = true;

            UdpData = new byte[PacketData.Length - 8];
            Array.Copy(PacketData, 8, UdpData, 0, PacketData.Length - 8);

            if (IsUdp && PacketData.Length > 8)
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
        public new string ToString()
        {
            return string.Join(Environment.NewLine, base.ToString(),
                string.Format("Source Port: {0} Destination Port: {1} UDP Length: {2} UDP Checksum: {3}", SourcePort, DestinationPort, UdpLength, UdpCheckSum),
                Environment.NewLine
            );
        }
        #endregion
    }
}


