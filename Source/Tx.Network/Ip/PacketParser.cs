namespace Tx.Network
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Class that provides methods for parsing of IP packets.
    /// </summary>
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
    public static class PacketParser
    {
        /// <summary>
        /// Parses the specified binary data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A new IP packet instance.</returns>
        /// <exception cref="System.ArgumentException">if data is empty.</exception>
        public static IpPacket Parse(ArraySegment<byte> data)
        {
            if (data.Count == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", "data");
            }

            return Parse(DateTimeOffset.UtcNow, true, data.Array, data.Offset, data.Count);
        }


        /// <summary>
        /// Parses the specified binary data.
        /// </summary>
        /// <param name="receivedTime">The received time.</param>
        /// <param name="reuseOriginalBuffer">if set to <c>true</c> then reuse original buffer.</param>
        /// <param name="packetBytes">The binary data.</param>
        /// <param name="offset">The offset in the binary data.</param>
        /// <param name="packetBytesLength">Length of the packet bytes.</param>
        /// <returns>A new IP packet instance.</returns>
        /// <exception cref="System.ArgumentNullException">packetBytes is empty.</exception>
        /// <exception cref="System.NotSupportedException">IPv4 only currently supported.</exception>
        public static IpPacket Parse(
            DateTimeOffset receivedTime, 
            bool reuseOriginalBuffer,
            byte[] packetBytes, 
            int offset, 
            int packetBytesLength)
        {
            if (packetBytes == null)
            {
                throw new ArgumentNullException("packetBytes");
            }

            var ipVers = packetBytes.ReadBits(offset, 0, 4);                    //bits 0 to 3
            if (ipVers != 4) throw new NotSupportedException("IPv4 only currently supported"); //ensure this is v4

            var internetHeaderLength = packetBytes.ReadBits(offset++, 4, 4);        //bits 4 to 7
            var dscpValue = packetBytes.ReadBits(offset, 0, 6);                     //8 to 13
            var explicitCongestionNotice = packetBytes.ReadBits(offset++, 6, 2);    //14 to 15
            var ipPacketLength = packetBytes.ReadNetOrderUShort(offset);            //16 to 31
            offset += 2;
            var fragmentGroupId = packetBytes.ReadNetOrderUShort(offset);           //32 to 47
            offset += 2;
            var ipHeaderFlags = packetBytes.ReadBits(offset, 0, 3);                 //48 to 50
            var fragmentOffset = packetBytes.ReadNetOrderUShort(offset, 3, 13);     //51 to 63
            offset += 2;
            var timeToLive = packetBytes[offset++];                                 //64 to 71
            var protocolNumber = packetBytes[offset++];                             //72 to 79
            var protocol = (ProtocolType)protocolNumber;                            //Enum
            var packetHeaderChecksum = packetBytes.ReadNetOrderUShort(offset);      //80 to 95
            offset += 2;
            var sourceIpAddress = packetBytes.ReadIpAddress(offset);                //96 to 127
            offset += 4;
            var destinationIpAddress = packetBytes.ReadIpAddress(offset);           //128 to 160
            offset += 4;

            var ipOptions = default(ArraySegment<byte>);
            if (internetHeaderLength > 5) //161 and up
            {
                int length = (internetHeaderLength - 5) * 4;

                if (reuseOriginalBuffer)
                {
                    ipOptions = new ArraySegment<byte>(packetBytes, offset, length);
                }
                else
                {
                    var ipOptionsDataArray = new byte[length];
                    Array.Copy(packetBytes, offset, ipOptionsDataArray, 0, length);
                    ipOptions = new ArraySegment<byte>(ipOptionsDataArray);
                }
                offset += length;
            }

            var packetData = default(ArraySegment<byte>);

            //IpHeader in bytes is 4*IHL bytes long
            if (ipPacketLength > 4 * internetHeaderLength)
            {
                int length = ipPacketLength - (internetHeaderLength * 4);

                if (reuseOriginalBuffer)
                {
                    packetData = new ArraySegment<byte>(packetBytes, offset, length);
                }
                else
                {
                    var packetDataArray = new byte[length];
                    Array.Copy(packetBytes, offset, packetDataArray, 0, length);
                    packetData = new ArraySegment<byte>(packetDataArray);
                }
            }

            return new IpPacket
            {
                PacketHeader = new IpPacketHeader(
                    sourceIpAddress,
                    destinationIpAddress,
                    false,
                    internetHeaderLength,
                    dscpValue,
                    explicitCongestionNotice,
                    ipPacketLength,
                    fragmentGroupId,
                    ipHeaderFlags,
                    fragmentOffset,
                    timeToLive,
                    packetHeaderChecksum),
                IpOptions = ipOptions,
                ReceivedTime = receivedTime,
                ProtocolType = protocol,
                PacketData = packetData
            };
        }
    }
}