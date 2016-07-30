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

        //Received DateTime TimeStamp
        public DateTimeOffset ReceivedTime { get; set; }

        //make all the public members read-only to prevent tomfoolery
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
            ReceivedTime = DateTime.UtcNow;
            IpVersion = NetworkInterfaceComponent.IPv4;
            Protocol = ProtocolType.Udp;
        }

        /// <summary>
        /// Used to construct a primitive IpPacket object with the intention to encode it for transmission on the network.
        /// </summary>
        /// <param name="IpVersion">IP version</param>
        /// <param name="InternetHeaderLength">IP header length</param>
        /// <param name="DscpValue">DSCP value to encode</param>
        /// <param name="ExplicitCongestionNotice">ECN value to encode</param>
        /// <param name="IpPacketLength">Length of the complete packet</param>
        /// <param name="FragmentGroupId">Fragement ID, may be 0</param>
        /// <param name="IpHeaderFlags">Header flags</param>
        /// <param name="FragmentOffset">Fragment offset, may be 0</param>
        /// <param name="TimeToLive">Internet TTL</param>
        /// <param name="Protocol">Protocol Type</param>
        /// <param name="SourceIpAddress">Source IPAddress, not assumed to be on the local host.</param>
        /// <param name="DestinationIpAddress">Destination IPAddress</param>
        /// <param name="IpOptions">IPOptions in Byte[] form</param>
        /// <param name="PacketData">The remainder of the packet.</param>
        /// <remarks>Note that the IP Header checksum would be computed when the ToWireBytes() method is called.</remarks>
       	public IpPacket(
             NetworkInterfaceComponent IpVersion,
             byte InternetHeaderLength,
             byte DscpValue,
             byte ExplicitCongestionNotice,
             ushort IpPacketLength,
             ushort FragmentGroupId,
             byte IpHeaderFlags,
             ushort FragmentOffset,
             byte TimeToLive,
             ProtocolType Protocol,
             
             IPAddress SourceIpAddress,
             IPAddress DestinationIpAddress,
             byte[] IpOptions,
             byte[] PacketData
            )
        {
            this.IpVersion = IpVersion;
            this.InternetHeaderLength = InternetHeaderLength;
            this.DscpValue = DscpValue;
            this.ExplicitCongestionNotice = ExplicitCongestionNotice;
            this.IpPacketLength = IpPacketLength;
            this.FragmentGroupId = FragmentGroupId;
            this.IpHeaderFlags = IpHeaderFlags;
            this.FragmentOffset = FragmentOffset;
            this.TimeToLive = TimeToLive;
            this.Protocol = Protocol;
            this.ProtocolNumber = (byte)Protocol;
            this.PacketHeaderChecksum = 0; //set to zero for new packet
            this.SourceIpAddress = SourceIpAddress;
            this.DestinationIpAddress = DestinationIpAddress;
            this.IpOptions = new byte[IpOptions.Length];
            Array.Copy(IpOptions, this.IpOptions, IpOptions.Length);
            this.PacketData = new byte[PacketData.Length];
            Array.Copy(PacketData, this.PacketData, PacketData.Length);

        }

        /// <summary>
        /// Produces a IpPacket based on input
        /// </summary>
        /// <param name="ReceivedDataBuffer">Incoming packet without alterations or prior processing </param>
        /// <returns> A new IpPacket. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on empty or null input byte[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too small -- minimum 20-bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on input byte[] too large -- maximum 65,535-bytes.</exception>
        public IpPacket(byte[] ReceivedDataBuffer)
        {

            if (ReceivedDataBuffer == null || ReceivedDataBuffer.Length == 0)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Received data buffer is empty or null");
            }

            else if (ReceivedDataBuffer.Length < 20)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Received data buffer is smaller than minimum IP packet length of header size of 20-bytes");
            }

            else if (ReceivedDataBuffer.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("ReceivedDataBuffer", "Received data buffer is larger than the maximum IP packet size of 65,535-bytes");
            }

            BuildPacket(ReceivedDataBuffer);
        }

        /// <summary>
        /// Produces a IpPacket based on input
        /// </summary>
        /// <param name="ReceivedPacket">IpPacket to copy to a new instance</param>
        /// <remarks> This method copies all data from the ReceivedPacket into a new packet, including byte arrays.</remarks>
        public IpPacket(IpPacket ReceivedPacket)
            : this()
        {
            ReceivedTime = ReceivedPacket.ReceivedTime;
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
            if (ReceivedPacket.IpOptions != null)
            {
                IpOptions = new byte[ReceivedPacket.IpOptions.Length];
                Array.Copy(ReceivedPacket.IpOptions, IpOptions, ReceivedPacket.IpOptions.Length);
            }
            if (ReceivedPacket.PacketData != null)
            {
                PacketData = new byte[ReceivedPacket.PacketData.Length];
                Array.Copy(ReceivedPacket.PacketData, PacketData, ReceivedPacket.PacketData.Length);
            }
            if (ReceivedPacket.DataBuffer != null)
            {
                DataBuffer = new byte[ReceivedPacket.DataBuffer.Length];
                Array.Copy(ReceivedPacket.DataBuffer, DataBuffer, ReceivedPacket.DataBuffer.Length);
            }
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
        private void BuildPacket(byte[] packetBytes)
        {
            int offset = 0;
            var ipVers = packetBytes.ReadBits(offset, 0, 4);                    //bits 0 to 3
            if (ipVers != 4) throw new NotSupportedException("IPv4 only currently supported"); //ensure this is v4
            InternetHeaderLength = packetBytes.ReadBits(offset++, 4, 4);        //bits 4 to 7
            DscpValue = packetBytes.ReadBits(offset, 0, 6);                     //8 to 13
            ExplicitCongestionNotice = packetBytes.ReadBits(offset++, 6, 2);    //14 to 15
            IpPacketLength = packetBytes.ReadNetOrderUShort(offset);            //16 to 31
            offset += 2;
            FragmentGroupId = packetBytes.ReadNetOrderUShort(offset);           //32 to 47
            offset += 2;
            IpHeaderFlags = packetBytes.ReadBits(offset, 0, 3);                 //48 to 50
            FragmentOffset = packetBytes.ReadNetOrderUShort(offset, 3, 13);     //51 to 63
            offset += 2;
            TimeToLive = packetBytes[offset++];                                 //64 to 71
            ProtocolNumber = packetBytes[offset++];                             //72 to 79
            Protocol = (ProtocolType)ProtocolNumber;                            //Enum
            PacketHeaderChecksum = packetBytes.ReadNetOrderUShort(offset);      //80 to 95
            offset += 2;
            SourceIpAddress = packetBytes.ReadIpAddress(offset);                //96 to 127
            offset += 4;
            DestinationIpAddress = packetBytes.ReadIpAddress(offset);           //128 to 160
            offset += 4;

            if (InternetHeaderLength > 5) //161 and up
            {
                int length = (InternetHeaderLength - 5) * 4;
                IpOptions = new byte[length];
                Array.Copy(packetBytes, offset, IpOptions, 0, length);
                offset += length;
            }

            //IpHeader in bytes is 4*IHL bytes long
            if (IpPacketLength > 4 * InternetHeaderLength)
            {
                int length = IpPacketLength - (InternetHeaderLength * 4);
                PacketData = new byte[length];
                Array.Copy(packetBytes, offset, PacketData, 0, length);
                offset += length;
            }
            DataBuffer = new byte[IpPacketLength];
            Array.Copy(packetBytes, 0, DataBuffer, 0, IpPacketLength);

        }

        #endregion
    }
}
