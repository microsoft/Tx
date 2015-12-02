namespace Tx.Network
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.IO;

    /// <summary>
    /// IP IpPacket Class Definition
    ///</summary>
    /// <remarks>
    ///     For many of the operations in this class a set of bits less than one (1) byte
    /// is needed. Bitwise operators and masks are needed to extract those bit values (0 or 1)
    /// from a byte.
    /// 
    ///     All data from the network is received as Big-Endian.Windows is Little-Endian.
    /// You can validate this with BitConverter.IsLittleEndian
    ///
    ///     Any sequence of bytes greater than 1 from the network has the most-significant
    /// byte at the LAST (aka End) position in the sequence. Network bytes are ALWAYS Big-Endian.
    ///
    ///         To do the "right thing" in Windows'-little-endian-world:
    ///             - A SINGULAR byte does not require any changes
    ///
    ///             - A CHARACTER array (aka string) or other Byte-Array requires no changes 
    ///             -- ASCII works this way.
    ///             -- Other encodings may vary so look up the rules.
    ///
    ///             - Any NUMBER comprised of more than 1 byte from the Network is Big-Endian
    ///             -- Therefore to make it work in Windows Network-to-Host-Order must be performed.
    ///             -- All numbers are even multiples of a byte ( greater than 1)
    ///             -- For example:
    ///                 | Network bytes will appear as [Ox80, 0x56] == 32854 (if cast to Int16 this will be a negative number)
    ///                 | NTHO in Windows will provide[0x56, 0x80] == 22144 
    ///
    ///         Please refer to the usage of 
    ///             -System.Net.IPAddress.NetworkToHostOrder(). 
    ///             -- NOTE this operates on SIGNED Integers(16,32,64) only.
    ///             -- All Internet header numbers are UNSIGNED Integers(16,32,64) so this makes no sense to me.
    ///             - System.BitConverter.ToInt16() or .ToInt32()
    ///
    ///        Typically the implementation would look like this: 
    ///             - (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 2));
    ///             - At zero-based-index 2 for (byte[])buffer of arbitrary size > 5 bytes 
    ///             - Return type is SHORT so a cast to USHORT is required while inside
    ///                 Internet Headers.
    ///         !!!
    ///         !WARNING: the Bytes in an IP-Address (v4 or v6) are READ as a Byte-Array and not a Number! 
    ///         !NToH Should not be used on Addresses!
    ///         !!!
    /// </remarks>
    /// 
    public class IpPacket
    {
        #region Public Fields
        //make all the public members readonly to prevent tomfoolery
        public NetworkInterfaceComponent IpVersion { get; private set; }
        public byte InternetHeaderLength { get; private set; }
        public byte DscpValue { get; private set; }
        public byte ExplicitCongestionNotice { get; private set; }
        public ushort IpPacketLength { get; private set; }
        public ushort FragmentGroupId { get; private set; }
        public byte IpHeaderFlags { get; private set; }
        public ushort FragmentOffset { get; private set; }
        public byte TimeToLive { get; private set; }
        public byte ProtocolNumber { get; private set; }
        public ProtocolType Protocol { get; private set; }
        public ushort PacketHeaderChecksum { get; private set; }
        public IPAddress SourceIpAddress { get; private set; }
        public IPAddress DestinationIpAddress { get; private set; }
        public byte[] IpOptions { get; private set; }
        public byte[] PacketData { get; private set; }
        public byte[] DataBuffer { get; private set; }

        #endregion

        #region Constructors

        public IpPacket()
        {
            IpVersion = NetworkInterfaceComponent.IPv4;
            Protocol = ProtocolType.Udp;
        }

        /// <summary>
        /// Produces a IpPacket based on input
        /// </summary>
        /// <param name="ReceivedDataBuffer">Incoming packet without alterations or prior processing </param>
        /// <returns> A new IpPacket. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on empty or null input byte[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too small -- minimum 20-bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too large -- maximum 65,535-bytes.</exception>
        public IpPacket(byte[] ReceivedDataBuffer) : this(new MemoryStream(ReceivedDataBuffer)) { }

        /// <summary>
        /// Produces a IpPacket based on input
        /// </summary>
        /// <param name="stream">Incoming packet in a MemoryStream without alterations or prior processing </param>
        /// <returns> A new IpPacket. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on empty or null input byte[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too small -- minimum 20-bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too large -- maximum 65,535-bytes.</exception>
        public IpPacket(Stream stream): this()
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Input byte[] is empty or null");
            }
            else if (stream.Length < 20)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Input byte[] is smaller than minimum IP packet length of header size of 20-bytes");
            }
            else if (stream.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Input byte[] is larger than the maximum IP packet size of 65,535-bytes");
            }
            BuildPacket(stream);
        }

        /// <summary>
        /// Produces a IpPacket based on input
        /// </summary>
        /// <param name="ReceivedPacket">IpPacket to copy to a new instance</param>
        /// <remarks> This method copies all data from the ReceivedPacket into a new packet, including byte arrays.</remarks>
        public IpPacket(IpPacket ReceivedPacket): this()
        {
            IpVersion = ReceivedPacket.IpVersion;
            InternetHeaderLength = ReceivedPacket.InternetHeaderLength;
            DscpValue = ReceivedPacket.DscpValue;
            ExplicitCongestionNotice = ReceivedPacket.ExplicitCongestionNotice;
            IpPacketLength = ReceivedPacket.IpPacketLength;
            FragmentGroupId = ReceivedPacket.FragmentGroupId;
            IpHeaderFlags = ReceivedPacket.IpHeaderFlags;
            FragmentOffset = ReceivedPacket.FragmentOffset;
            TimeToLive = ReceivedPacket.TimeToLive;
            ProtocolNumber = ReceivedPacket.ProtocolNumber;
            Protocol = ReceivedPacket.Protocol;
            PacketHeaderChecksum = ReceivedPacket.PacketHeaderChecksum;
            SourceIpAddress = new IPAddress(ReceivedPacket.SourceIpAddress.GetAddressBytes());
            DestinationIpAddress = new IPAddress(ReceivedPacket.DestinationIpAddress.GetAddressBytes());
            IpOptions = new byte[ReceivedPacket.IpOptions.Length];
            PacketData = new byte[ReceivedPacket.PacketData.Length];
            DataBuffer = new byte[ReceivedPacket.DataBuffer.Length];
            Array.Copy(ReceivedPacket.IpOptions, IpOptions, ReceivedPacket.IpOptions.Length);
            Array.Copy(ReceivedPacket.PacketData, PacketData, ReceivedPacket.PacketData.Length);
            Array.Copy(ReceivedPacket.DataBuffer, DataBuffer, ReceivedPacket.DataBuffer.Length);

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a string representation of the IpPacket object.
        /// </summary>
        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("IP version: ");
            s.AppendLine(IpVersion.ToString());
            s.Append("Internet Header Length ");
            s.AppendLine(InternetHeaderLength.ToString());
            s.Append("DSCP value ");
            s.AppendLine(DscpValue.ToString());
            s.Append("ECN value ");
            s.AppendLine(ExplicitCongestionNotice.ToString());
            s.Append("IP packet length ");
            s.AppendLine(IpPacketLength.ToString());
            s.Append("ID/Fragment Group ");
            s.AppendLine(FragmentGroupId.ToString());
            s.Append("IP header flags ");
            s.AppendLine(IpHeaderFlags.ToString());
            s.Append("Fragment offset ");
            s.AppendLine(FragmentOffset.ToString());
            s.Append("TTL ");
            s.AppendLine(TimeToLive.ToString());
            s.Append("Protocol Number ");
            s.AppendLine(ProtocolNumber.ToString());
            s.Append("Header Checksum ");
            s.AppendLine(PacketHeaderChecksum.ToString());
            s.Append("Source IP ");
            s.AppendLine(SourceIpAddress.ToString());
            s.Append("Destination IP ");
            s.AppendLine(DestinationIpAddress.ToString());
            if (IpOptions != null)
            {
                s.Append("Length of IP options ");
                s.AppendLine(IpOptions.Length.ToString());
            }
            s.Append("Packet Data Length ");
            s.AppendLine(PacketData.Length.ToString());
            s.Append("Size of data buffer processed ");
            s.AppendLine(DataBuffer.Length.ToString());

            return s.ToString();
        }
        #endregion

        #region Private Methods
        private void BuildPacket(byte[] Buffer)
        {
            using(var stream = new MemoryStream(Buffer)) BuildPacket(stream);
        }
        private void BuildPacket(Stream Stream)
        {

            var packetBytes = new BinaryReader(Stream);

            var ipVers = packetBytes.ReadBits(0, 4);                            //bits 0 to 3
            if (ipVers != 4) throw new NotSupportedException("IPv4 only currently supported"); //ensure this is v4
            InternetHeaderLength = packetBytes.ReadBits(4, 4, true);            //bits 4 to 7
            DscpValue = packetBytes.ReadBits(0, 6);                             //8 to 13
            ExplicitCongestionNotice = packetBytes.ReadBits(6, 2, true);        //14 to 15
            IpPacketLength = packetBytes.ReadNetOrderUShort();                  //16 to 31
            FragmentGroupId = packetBytes.ReadNetOrderUShort();                 //32 to 47
            IpHeaderFlags = packetBytes.ReadBits(0, 3);                         //48 to 50
            FragmentOffset = packetBytes.ReadNetOrderUShort(3, 13);             //51 to 63
            TimeToLive = packetBytes.ReadByte();                                //64 to 71
            ProtocolNumber = packetBytes.ReadByte();                            //72 to 79
            Protocol = (ProtocolType)ProtocolNumber;                            //Enum
            PacketHeaderChecksum = packetBytes.ReadNetOrderUShort();            //80 to 95
            SourceIpAddress = packetBytes.ReadIpAddress();                      //96 to 127
            DestinationIpAddress = packetBytes.ReadIpAddress();                 //128 to 160

            if (InternetHeaderLength > 5) //161 and up
            {
                IpOptions = new byte[(InternetHeaderLength - 5) * 4];
                packetBytes.Read(IpOptions, 0, (InternetHeaderLength - 5) * 4);
            }
            else
            {
                IpOptions = null;
            }
            //IpHeader in bytes is 4*IHL bytes long
            if (IpPacketLength > 4 * InternetHeaderLength)
            {
                PacketData = new byte[IpPacketLength - (InternetHeaderLength * 4)];
                packetBytes.Read(PacketData, 0, IpPacketLength - (InternetHeaderLength * 4));
            }
            else
            {
                PacketData = null; //sometimes the datagram is empty
            }
        }
        #endregion
    }
}