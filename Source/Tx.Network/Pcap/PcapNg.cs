// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// The Pcap Next Generation format is: https://www.winpcap.org/ntar/draft/PCAP-DumpFileFormat.html
// The C# implementation below reads files in .pcapng format

namespace Tx.Network
{
    public class PcapNg
    {
        /// <summary>
        /// Reads network capture file and returns the raw blocks in the order they were written
        /// </summary>
        /// <param name="filename">Path to the file in pcap-next-generation (.pcapng) format</param>
        /// <returns></returns>
        public static IEnumerable<Block> ReadForward(string filename)
        {
            var stream = File.OpenRead(filename);
            return ReadForward(stream);
        }

        /// <summary>
        /// Reads a stream and returns the raw blocks in the order they were written
        /// </summary>
        /// <param name="stream">Stream in pcap-next-generation format</param>
        /// <returns></returns>
        public static IEnumerable<Block> ReadForward(Stream stream)
        {
            var interfaces = new List<InterfaceDescriptionBlock>();

            using (var reader = new BinaryReader(stream))
            {
                while (true)
                {
                    if (stream.Position == stream.Length)
                        yield break;

                    BlockType type = (BlockType)reader.ReadUInt32();
                    UInt32 length = reader.ReadUInt32();

                    switch (type)
                    {
                        case BlockType.SectionHeaderBlock:
                            yield return new SectionHeaderBlock(type, length, reader);
                            break;

                        case BlockType.InterfaceDescriptionBlock:
                            var interfacceDesc =  new InterfaceDescriptionBlock(type, length, reader);
                            interfaces.Add(interfacceDesc);
                            yield return interfacceDesc;
                            break;

                        case BlockType.EnhancedPacketBlock:
                            yield return new EnhancedPacketBlock(type, length, reader, interfaces);
                            break;

                        default:
                            yield return new GenericBlock(type, length, reader);
                            break;
                    }
                }
            }
        }
    }
    public enum BlockType
    {
        Reserved = 0,
        InterfaceDescriptionBlock = 0x00000001,
        PacketBlock = 0x00000002,
        SimplePacketBlock = 0x00000003,
        NameResolutionBlock = 0x00000004,
        InterfaceStatisticsBlock = 0x00000005,
        EnhancedPacketBlock = 0x00000006,
        IRIGTimestampBlock = 0x00000007,
        SectionHeaderBlock = 0x0A0D0D0A

    }    

    public abstract class Block
    {
        public BlockType Type { get; private set; }
        public UInt32 Length { get; private set;}
        protected Block(BlockType type, UInt32 length) // called by the derived classes
        {
            Type = type;
            Length = length;
        }
        /// <summary>
        /// The inheriting classes must call this as last line in their constructor
        /// </summary>
        protected void ReadEndOfPacket(BinaryReader reader)
        {
            // Each packet ends with the same value of Block Total Length, 
            // which is intended to support backward reading.
            // Reading this length is also useful to make sure the logic in the inheriting class read the block correctly
            UInt32 length2 = reader.ReadUInt32();
            if (length2 != Length)
                throw new Exception("The Toatal Block Length at the end of bock " + Enum.GetName(typeof(BlockType), this.Type) +" does not match the length");
        }

        protected string ReadAsciiOption(BinaryReader reader, int len)
        {
            int readLen = len + (4 - len % 4);
            byte[] bytes = reader.ReadBytes(readLen);
            string s = Encoding.ASCII.GetString(bytes, 0, len);
            return s;
        }

        protected byte[] ReadBytesOption(BinaryReader reader, int len)
        {
            int readLen = len + (4 - len % 4);
            byte[] bytes = reader.ReadBytes(readLen);
            return bytes;
        }
    }

    /// <summary>
    /// Represents block that does not have specific parsing implemented
    /// </summary>
    public class GenericBlock : Block
    {
        public byte[] Body;

        internal GenericBlock(BlockType type, UInt32 length, BinaryReader reader)
            : base(type, length)
        {
            Body = reader.ReadBytes((int)Length - 12);

            ReadEndOfPacket(reader);
        }

    }
    public class EnhancedPacketBlock : Block
    {
        public InterfaceDescriptionBlock InterfaceDescription { get; private set; }
        public UInt32 CapturedLen { get; private set; }
        public UInt32 PacketLen { get; private set; }
        public byte[] PacketData { get; private set; }

        public DateTime TimestampUtc { get; private set; }
        internal EnhancedPacketBlock(BlockType type, UInt32 length, BinaryReader reader, List<InterfaceDescriptionBlock> interfaces)
            : base(type, length)
        {
            int interfaceID = reader.ReadInt32();
            InterfaceDescription = interfaces[interfaceID];
            long timestampHigh = reader.ReadUInt32();
            long timestampLow = reader.ReadUInt32();
            long offset = (new DateTime(1970, 1, 1) - new DateTime(1601, 1, 1)).Ticks;
            long ticks = offset + ((timestampHigh << 32) | timestampLow) * InterfaceDescription.TimeMultiplier; 
            TimestampUtc = DateTime.FromFileTime(ticks);
            CapturedLen = reader.ReadUInt32();
            PacketLen = reader.ReadUInt32();
            PacketData = reader.ReadBytes((int)CapturedLen);
            uint optionsLen = Length - 8*4 - CapturedLen;
            byte[] options = reader.ReadBytes((int)optionsLen);

            ReadEndOfPacket(reader);
        }
    }

    public class SectionHeaderBlock : Block
    {
        public UInt16 MajorVersion { get; private set; }
        public UInt16 MinorVersion { get; private set; }
        public UInt64 SectionLength { get; private set; }

        internal SectionHeaderBlock(BlockType type, UInt32 length, BinaryReader reader)
            : base(type, length)
        {
            UInt32 byteOrderMagic = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            SectionLength = reader.ReadUInt64();
            uint optionsLen = Length - 7 * 4;
            byte[] options = reader.ReadBytes((int)optionsLen);

            ReadEndOfPacket(reader);
        }
    }

    public enum LinkType
    {
        /// <summary>
        /// // No link layer information. A packet saved with this link layer contains a raw L3 packet preceded by a 32-bit host-byte-order AF_ value indicating the specific L3 type. 
        /// </summary>
        NULL = 0, 
        /// <summary>
        /// D/I/X and 802.3 Ethernet 
        /// </summary>
        ETHERNET = 1, 
        /// <summary>
        /// Experimental Ethernet (3Mb)
        /// </summary>
        EXP_ETHERNET = 2,
        /// <summary>
        /// Amateur Radio AX.25
        /// </summary>
        AX25 = 3,
        /// <summary>
        /// Proteon ProNET Token Ring
        /// </summary>
        PRONET = 4,
        /// <summary>
        /// Chaos
        /// </summary>
        CHAOS = 5,
        /// <summary>
        /// IEEE 802 Networks
        /// </summary>
        TOKEN_RING =6, 
        /// <summary>
        /// ARCNET, with BSD-style header
        /// </summary>
        ARCNET =7,
        /// <summary>
        /// Serial Line IP
        /// </summary>
        SLIP =8,
        /// <summary>
        /// Point-to-point Protocol 
        /// </summary>
        PPP =9,
        /// <summary>
        /// FDDI
        /// </summary>
        FDDI =10,
        /// <summary>
        /// PPP in HDLC-like framing
        /// </summary>
        PPP_HDLC =50,
        /// <summary>
        /// NetBSD PPP-over-Ethernet
        /// </summary>
        PPP_ETHER =51,
        /// <summary>
        /// Symantec Enterprise Firewall
        /// </summary>
        SYMANTEC_FIREWALL =99,
        /// <summary>
        /// LLC/SNAP-encapsulated ATM
        /// </summary>
        ATM_RFC1483 =100,
        /// <summary>
        /// Raw IP
        /// </summary>
        RAW = 101,
        /// <summary>
        /// BSD/OS SLIP BPF header
        /// </summary>
        SLIP_BSDOS =102,
        /// <summary>
        /// BSD/OS PPP BPF header
        /// </summary>
        PPP_BSDOS =103, 
        /// <summary>
        /// Cisco HDLC 
        /// </summary>
        C_HDLC =104,
        /// <summary>
        /// IEEE 802.11 (wireless)
        /// </summary>
        IEEE802_11 =105,
        /// <summary>
        /// Linux Classical IP over ATM
        /// </summary>
        ATM_CLIP =106, 
        /// <summary>
        /// Frame Relay
        /// </summary>
        FRELAY =107, 
        /// <summary>
        /// OpenBSD loopback
        /// </summary>
        LOOP =108, 
        /// <summary>
        /// OpenBSD IPSEC enc
        /// </summary>
        ENC =109, 
        /// <summary>
        /// ATM LANE + 802.3 (Reserved for future use)
        /// </summary>
        LANE8023 =110, 
        /// <summary>
        /// NetBSD HIPPI (Reserved for future use)
        /// </summary>
        HIPPI =111,
        /// <summary>
        /// NetBSD HDLC framing (Reserved for future use)
        /// </summary>
        HDLC =112,
        /// <summary>
        /// Linux cooked socket capture
        /// </summary>
        LINUX_SLL =113,
        /// <summary>
        /// Apple LocalTalk hardware
        /// </summary>
        LTALK =114,
        /// <summary>
        /// Acorn Econet
        /// </summary>
        ECONET =115,
        /// <summary>
        /// Reserved for use with OpenBSD ipfilter
        /// </summary>
        IPFILTER =116,
        /// <summary>
        /// OpenBSD DLT_PFLOG
        /// </summary>
        PFLOG =117,
        /// <summary>
        /// For Cisco-internal use
        /// </summary>
        CISCO_IOS =118,
        /// <summary>
        /// 802.11+Prism II monitor mode
        /// </summary>
        PRISM_HEADER =119,
        /// <summary>
        /// FreeBSD Aironet driver stuff
        /// </summary>
        AIRONET_HEADER =120,
        /// <summary>
        /// Reserved for Siemens HiPath HDLC
        /// </summary>
        HHDLC =121,
        /// <summary>
        /// RFC 2625 IP-over-Fibre Channel 
        /// </summary>
        IP_OVER_FC =122, 
        /// <summary>
        /// Solaris+SunATM
        /// </summary>
        SUNATM =123, 
        /// <summary>
        /// RapidIO
        /// </summary>
        RIO =124,
        /// <summary>
        /// PCI Express
        /// </summary>
        PCI_EXP =125,
        /// <summary>
        /// Xilinx Aurora link layer
        /// </summary>
        AURORA =126,
        /// <summary>
        /// 802.11 plus BSD radio header
        /// </summary>
        IEEE802_11_RADIO =127, 
        /// <summary>
        /// Tazmen Sniffer Protocol - Reserved for the TZSP encapsulation, as per request from Chris Waters &lt;chris.waters@networkchemistry.com&gt; TZSP is a generic encapsulation for any other link type, which includes a means to include meta-information with the packet, e.g. signal strength and channel for 802.11 packets. 
        /// </summary>
        TZSP =128,
        /// <summary>
        /// Linux-style headers
        /// </summary>
        ARCNET_LINUX =129,
        JUNIPER_MLPPP =130,
        JUNIPER_MLFR =131, 
        JUNIPER_ES =132,
        JUNIPER_GGSN =133,
        JUNIPER_MFR =134,
        JUNIPER_ATM2 =135,
        JUNIPER_SERVICES =136,
        JUNIPER_ATM1 =137,
        /// <summary>
        /// Apple IP-over-IEEE 1394 cooked header
        /// </summary>
        APPLE_IP_OVER_IEEE1394 =138,
        MTP2_WITH_PHDR =139,
        MTP2 =140, 
        MTP3 =141, 
        SCCP =142,
        /// <summary>
        /// DOCSIS MAC frames
        /// </summary>
        DOCSIS =143,
        /// <summary>
        /// Linux-IrDA
        /// </summary>
        LINUX_IRDA =144,
        /// <summary>
        /// Reserved for IBM SP switch and IBM Next Federation switch.
        /// </summary>
        IBM_SP =145,
        /// <summary>
        /// Reserved for IBM SP switch and IBM Next Federation switch.
        /// </summary>
        IBM_SN =146  
    }

    public class InterfaceDescriptionBlock : Block
    {
        public LinkType LinkType { get; private set; }
        public UInt32 SnapLen { get; private set; }
        /// <summary>
        /// String containing the name of the device used to capture data.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// string containing the description of the device used to capture data.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Number to multiply 64 bit timestamps to obtain Ticks for DateTime
        /// </summary>
        public long TimeMultiplier { get; private set; }
        internal InterfaceDescriptionBlock(BlockType type, UInt32 length, BinaryReader reader)
            : base(type, length)
        {
            LinkType = (LinkType)reader.ReadUInt16();
            var reserved = reader.ReadUInt16();
            SnapLen = reader.ReadUInt32();
            uint optionsLen = Length - 5 * 4;
            //byte[] options = reader.ReadBytes((int)optionsLen);

            TimeMultiplier = 10; // default value of 10^-6 if the option if_tsresol is not present
            int optionCode;

            while(true) // Options can occur in any order, so we have too loop
            {
                optionCode = reader.ReadInt16();
                int optionLength = reader.ReadInt16();
                if (optionCode == 0)
                    break;

                switch (optionCode)
                {
                    case 2:
                        Name = ReadAsciiOption(reader, optionLength);
                        continue;

                    case 3:
                        Description = ReadAsciiOption(reader, optionLength);
                        continue;

                    case 9:
                        byte[] buffer = ReadBytesOption(reader, 1);
                        byte b = buffer[0];
                        if ((b & 0x80) == 0)
                            TimeMultiplier = (long)(10E6 / Math.Pow(10, b));
                        else
                            TimeMultiplier = (long)(10E6 / Math.Pow(2, b));
                        continue;

                    default: // This is some unsupported option, but we still havo to read to skip it
                        ReadBytesOption(reader, optionLength);
                        continue;
                }
            } 

           ReadEndOfPacket(reader);
        }
    }
}
