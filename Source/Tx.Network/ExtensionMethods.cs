namespace Tx.Network
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extentions to Byte[] to: 
    ///     - read bits at specified offsets from either a Byte or a network order UShort
    ///     - read a single Byte or network order UShort
    ///     - read an IPv4 Address
    /// </summary>
    internal static class ByteArrayExtentions
    {
        public static byte ReadBits(this byte[] bytes, int bufferOffset, int bitPosition, int bitLength)
        {
            return bytes[bufferOffset].ReadBits(bitPosition, bitLength);
        }

        public static byte ReadBits(this byte bytes, int bitPosition, int bitLength)
        {
            var bitShift = 8 - bitPosition - bitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 8 for byte output type.");
            }
            return (byte)(((0xff >> (bitPosition)) & bytes) >> bitShift);
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int bufferOffset, int bitPosition, int bitLength)
        {
            var bitShift = 16 - bitPosition - bitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 16 for ushort output type.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(((0xffff >> bitPosition) & BitConverter.ToUInt16(bytes, bufferOffset) >> bitShift));
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int bufferOffset)
        {
            if (bytes.Length - bufferOffset < 2)
            {
                throw new ArgumentOutOfRangeException("Buffer offset overflows size of byte array.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, bufferOffset));
        }
        public static ushort ReadUShort(this byte[] bytes, int bufferOffset)
        {
            if (bytes.Length - bufferOffset < 2)
            {
                throw new ArgumentOutOfRangeException("Buffer offset overflows size of byte array.");
            }
            return (ushort)BitConverter.ToInt16(bytes, bufferOffset);
        }
        public static IPAddress ReadIpAddress(this byte[] bytes, int bufferOffset)
        {
            var ipBytes = new byte[4];
            Array.Copy(bytes, bufferOffset, ipBytes, 0, 4);
            return new IPAddress(ipBytes);
        }

        public static ArraySegment<byte> AsByteArraySegment(this IReadOnlyCollection<byte> source)
        {
            if (source is ArraySegment<byte>)
            {
                return (ArraySegment<byte>)source;
            }

            var bytes = source as byte[];
            if (bytes != null)
            {
                return new ArraySegment<byte>(bytes);
            }

            return new ArraySegment<byte>(source.ToArray());
        }
    }
}


