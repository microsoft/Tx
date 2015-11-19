namespace Tx.Network
{
    using System;
    using System.Net;
    using System.IO;


    /// <summary>
    /// Extentions to Byte[] to: 
    ///     - read bits at specified offsets from either a Byte or a network order UShort
    ///     - read a single Byte or network order UShort
    ///     - read an IPv4 Address
    /// </summary>
    public static class ByteArrayExtentions
    {
        #region Byte[] Extentions
        public static byte ReadBits(this byte[] bytes, int BufferOffset, int BitPosition, int BitLength)
        {
            return bytes[BufferOffset].ReadBits(BitPosition, BitLength);
        }
        public static byte ReadBits(this byte bytes, int BitPosition, int BitLength)
        {
            var bitShift = 8 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 8 for byte output type.");
            }
            return (byte)(((0xff >> (BitPosition)) & bytes) >> bitShift);
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int BufferOffset, int BitPosition, int BitLength)
        {
            var bitShift = 16 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 16 for ushort output type.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(((0xffff >> BitPosition) & BitConverter.ToUInt16(bytes, BufferOffset) >> bitShift));
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int BufferOffset)
        {
            if (bytes.Length - BufferOffset < 2)
            {
                throw new ArgumentOutOfRangeException("Buffer offset overflows size of byte array.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, BufferOffset));
        }
        public static IPAddress ReadIpAddress(this byte[] bytes, int BufferOffset)
        {
            var IpBytes = new byte[4];
            Array.Copy(bytes, BufferOffset, IpBytes, 0, 4);
            return new IPAddress(IpBytes);
        }
        #endregion
    }
    /// <summary>
    /// Extentions to BinaryReader to: 
    ///     - read bits at specified offsets from either a Byte or a network order UShort
    ///     - read a single Byte or network order UShort
    ///     - read an IPv4 Address
    ///     - look at the current byte without moving the the position in the stream
    /// </summary>
    public static class BinaryReaderExtentions
    {
        #region BinaryReader Extentions
        public static byte PeekByte(this BinaryReader bytes)
        {
            var pos = bytes.BaseStream.Position;
            var b = bytes.ReadByte();
            bytes.BaseStream.Seek(pos, 0);
            return b;
        }
        public static byte ReadBits(this BinaryReader bytes, int BitPosition, int BitLength, bool Advance = false)
        {
            if (Advance) return bytes.ReadByte().ReadBits(BitPosition, BitLength);
            return bytes.PeekByte().ReadBits(BitPosition, BitLength);
        }
        public static ushort ReadNetOrderUShort(this BinaryReader bytes)
        {
            return (ushort)IPAddress.NetworkToHostOrder(bytes.ReadInt16());
        }
        public static ushort ReadNetOrderUShort(this BinaryReader bytes, int BitPosition, int BitLength)
        {
            var bitShift = 16 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new  ArgumentOutOfRangeException("BitPostion + BitLength greater than 16 for ushort output type.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(((0xffff >> BitPosition)) & bytes.ReadUInt16() >> bitShift);
        }
        public static IPAddress ReadIpAddress(this BinaryReader bytes)
        {
            return new IPAddress(bytes.ReadBytes(4));
        }
        #endregion
    }
}
