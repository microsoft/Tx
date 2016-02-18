namespace Tx.Network.Snmp
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Internal class which provides Asn1 Encoding extension methods
    /// </summary>
    internal static class Asn1EncoderExtensions
    {
        private const uint uintMask = 0xFF800000u;
        private const ulong ulongMask = 0xFF80000000000000;
        private const int intShiftSize = 8 * (sizeof(int) - 1);
        private const int uintShiftSize = 8 * (sizeof(uint) - 1);
        private const int ulongShiftSize = 8 * (sizeof(ulong) - 1);

        /// <summary>
        /// Encodes the type of the class construct.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="asn1Class">The asn1 class.</param>
        /// <param name="constructType">Type of the construct.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>new offset value in int</returns>
        public static int EncodeClassConstructType(this byte[] data, int offset, Asn1Class asn1Class, ConstructType constructType, byte tag)
        {
            data[offset] = (byte)(((byte)asn1Class << 6) | ((byte)constructType << 5) | tag);
            return offset + 1;
        }

        /// <summary>
        /// Encodes the length.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="lengthIntegerToEncode">The length integer to encode.</param>
        /// <returns>encoded length</returns>
        public static int EncodeLength(this byte[] data, int offset, int lengthIntegerToEncode)
        {
            if (lengthIntegerToEncode <= 127)
            {
                data[offset] = (byte)lengthIntegerToEncode;
                return offset + 1;
            }

            int lengthOfLength = 0;
            uint mask = 0xFF000000;
            bool foundNonZero = false;
            for (int i = 1; i <= sizeof(int); i++)
            {
                if (foundNonZero || (mask & lengthIntegerToEncode) != 0)
                {
                    lengthOfLength++;
                    data[offset + lengthOfLength] = (byte)((lengthIntegerToEncode & mask) >> ((sizeof(int) - i) * 8));
                    foundNonZero = true;
                }
                mask >>= 8;
            }

            data[offset] = (byte)(0x80 + lengthOfLength);
            return offset + lengthOfLength + 1;
        }

        /// <summary>
        /// Encodes the ip address.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns>offset as int</returns>
        private static int EncodeIPAddress(this byte[] bytes, int offset, System.Net.IPAddress ipAddress)
        {
            offset = bytes.EncodeClassConstructType(offset, Asn1Class.Application, ConstructType.Primitive, (byte)Asn1SnmpTag.IpAddress);
            offset = bytes.EncodeLength(offset, 4);

            byte[] bytesEncoded = ipAddress.GetAddressBytes();
            Array.Copy(bytesEncoded, 0, bytes, offset, bytesEncoded.Length);
            return offset + 4;
        }

        /// <summary>
        /// Gets the length of the integer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static int GetIntegerLength(int value)
        {
            uint mask = 0xFF800000u;
            int size = sizeof(int);
            while (((value & mask) == 0 || (value & mask) == mask) && size > 1)
            {
                size--;
                value <<= 8;
            }

            return size;
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        /// <param name="octetString">The octet string.</param>
        /// <returns></returns>
        private static int GetStringLength(string octetString)
        {
            if (octetString == null)
            {
                return 0;
            }

            // does not support long string
            if (octetString.Length > 75)
            {
                return 75;
            }

            return octetString.Length;
        }

        /// <summary>
        /// Encodes the null.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>offset as int</returns>
        public static int EncodeNull(this byte[] data, int offset)
        {
            offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.Null);
            return data.EncodeLength(offset, GetIntegerLength(0)) + 1;
        }

        /// <summary>
        /// Encodes the variable binds.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="varBinds">The variable binds.</param>
        /// <returns>int offset</returns>
        public static int EncodeVarBinds(this byte[] data, int offset, ReadOnlyCollection<VarBind> varBinds)
        {
            int length = offset;
            int tempOffset = 0;
            for(int i=0; i< varBinds.Count; i++)
            {
                offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Constructed, (byte)Asn1Tag.Sequence);
                tempOffset = data.EncodeOid(offset + 1, varBinds[i].Oid.Oids);

                if (varBinds[i].Asn1TypeInfo.Asn1SnmpTagType == Asn1SnmpTag.NotSnmpData)
                {
                    switch(varBinds[i].Asn1TypeInfo.Asn1TagType)
                    {
                        case Asn1Tag.Integer:
                            {
                                tempOffset = data.EncodeLongInteger(tempOffset, Convert.ToInt64(varBinds[i].Value));
                                break;
                            }
                        case Asn1Tag.Null:
                            {
                                tempOffset = data.EncodeNull(tempOffset);
                                break;
                            }
                        case Asn1Tag.OctetString:
                            {
                                tempOffset = data.EncodeOctetString(tempOffset, (string)varBinds[i].Value);
                                break;
                            }
                        case Asn1Tag.ObjectIdentifier:
                            {
                                tempOffset = data.EncodeOid(tempOffset, ((ObjectIdentifier)varBinds[i].Value).Oids);
                                break;
                            }
                        default:
                            {
                                tempOffset = data.EncodeNull(tempOffset);
                                break;
                            }
                    }
                }
                else
                {
                    switch (varBinds[i].Asn1TypeInfo.Asn1SnmpTagType)
                    {
                        case Asn1SnmpTag.Counter:
                            {
                                tempOffset = data.EncodeUnsignedInteger(tempOffset, (uint)varBinds[i].Value, (byte)Asn1SnmpTag.Counter);
                                break;
                            }
                        case Asn1SnmpTag.IpAddress:
                            {
                                tempOffset = data.EncodeIPAddress(tempOffset, (System.Net.IPAddress)varBinds[i].Value);
                                break;
                            }
                        case Asn1SnmpTag.Counter64:
                            {
                                tempOffset = data.EncodeUnsignedLong(tempOffset, (ulong)varBinds[i].Value, (byte)Asn1SnmpTag.Counter64);
                                break;
                            }
                        case Asn1SnmpTag.Gauge:
                            {
                                tempOffset = data.EncodeUnsignedInteger(tempOffset, (uint)varBinds[i].Value, (byte)Asn1SnmpTag.Gauge);
                                break;
                            }
                        case Asn1SnmpTag.TimeTicks:
                            {
                                tempOffset = data.EncodeUnsignedInteger(tempOffset, (uint)varBinds[i].Value, (byte)Asn1SnmpTag.TimeTicks);
                                break;
                            }
                        case Asn1SnmpTag.UInt32:
                            {
                                tempOffset = data.EncodeUnsignedInteger(tempOffset, (uint)varBinds[i].Value, (byte)Asn1SnmpTag.UInt32);
                                break;
                            }
                        default:
                            {
                                tempOffset = data.EncodeNull(tempOffset);
                                break;
                            }
                    }
                }

                data.EncodeLength(offset, tempOffset - offset);
                offset = tempOffset;
            }

            return offset - length;
        }

        /// <summary>
        /// Encodes the integer.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns>offset as int</returns>
        public static int EncodeInteger(this byte[] data, int offset, int value)
        {
            offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.Integer);

            int size = sizeof(int);
            while (((value & uintMask) == 0 || (value & uintMask) == uintMask) && size > 1)
            {
                size--;
                value <<= 8;
            }

            offset = data.EncodeLength(offset, size);
            while (size-- > 0)
            {
                data[offset++] = (byte)((value & uintMask) >> intShiftSize);
                value <<= 8;
            }
            return offset;
        }

        /// <summary>
        /// Encodes the long integer.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns>int offset</returns>
        public static int EncodeLongInteger(this byte[] data, int offset, long value)
        {
            byte[] buffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.Integer);
            offset = data.EncodeLength(offset, buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                data[offset++] = buffer[i];
            }

            return offset;

        }

        /// <summary>
        /// Encodes the unsigned integer.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>int offset</returns>
        private static int EncodeUnsignedInteger(this byte[] data, int offset, uint value, byte tag)
        {
            offset = data.EncodeClassConstructType(offset, Asn1Class.Application, ConstructType.Primitive, tag);
         
            int size = sizeof(uint);
            while (((value & uintMask) == 0 || (value & uintMask) == uintMask) && size > 1)
            {
                size--;
                value <<= 8;
            }

            offset = data.EncodeLength(offset, size);
            while (size-- > 0)
            {
                data[offset++] = (byte)((value & uintMask) >> uintShiftSize);
                value <<= 8;
            }

            return offset;
        }

        /// <summary>
        /// Encodes the unsigned long.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>int offset</returns>
        private static int EncodeUnsignedLong(this byte[] data, int offset, ulong value, byte tag)
        {
            offset = data.EncodeClassConstructType(offset, Asn1Class.Application, ConstructType.Primitive, tag);

            int size = sizeof(ulong);
            while (((value & ulongMask) == 0 || (value & ulongMask) == ulongMask) && size > 1)
            {
                size--;
                value <<= 8;
            }

            offset = data.EncodeLength(offset, size);
            while (size-- > 0)
            {
                data[offset++] = (byte)((value & ulongMask) >> ulongShiftSize);
                value <<= 8;
            }
            return offset;
        }

        /// <summary>
        /// Sizes the of sub identifier.
        /// </summary>
        /// <param name="subid">The subid.</param>
        /// <returns>int size of SubID</returns>
        private static int SizeOfSubID(uint subid)
        {
            if (subid < 0x80u)
            {
                return 1;
            }
            else if (subid < 0x4000u)
            {
                return 2;
            }
            else if (subid < 0x200000u)
            {
                return 3;
            }
            else if (subid < 0x10000000u)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }

        /// <summary>
        /// Gets the length of the oid.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <returns>int length</returns>
        private static int GetOidLength(ReadOnlyCollection<uint> oid)
        {
            if (oid.Count <= 0)
            {
                return 0;
            }

            if (oid.Count == 1)
            {
                return 1;
            }

            uint subId = (oid[0] * 40) + oid[1];
            int size = SizeOfSubID(subId);
            for (int i = 2; i < oid.Count; i++)
            {
                size += SizeOfSubID(oid[i]);
            }

            return size;
        }

        /// <summary>
        /// Encodes the octet string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="octetString">The octet string.</param>
        /// <returns>int offset</returns>
        public static int EncodeOctetString(this byte[] data, int offset, string octetString)
        {
            int len = GetStringLength(octetString);
            offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.OctetString);
            offset = data.EncodeLength(offset, len);

            for (int i = 0; i < len; i++)
            {
                data[offset++] = (byte)octetString[i];
            }

            return offset;
        }

        /// <summary>
        /// Encodes the oid.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="subOids">The sub oids.</param>
        /// <returns>int offset</returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        private static int EncodeOid(this byte[] data, int offset, ReadOnlyCollection<uint> subOids)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            offset = data.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.ObjectIdentifier);
            offset = data.EncodeLength(offset, GetOidLength(subOids));

            offset = EncodeSubID((subOids[0] * 40) + subOids[1], data, offset);

            for (int i = 2; i < subOids.Count; i++)
            {
                offset = EncodeSubID(subOids[i], data, offset);
            }
            return offset;
        }

        /// <summary>
        /// Encodes the sub identifier.
        /// </summary>
        /// <param name="subid">The subid.</param>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>int offset</returns>
        private static int EncodeSubID(uint subid, byte[] data, int offset)
        {
            if (subid < 0x80u)
            { // Size = 1
                data[offset++] = (byte)subid;
            }
            else if (subid < 0x4000u)
            { // Size = 2
                data[offset++] = (byte)((subid >> 7) | 0x80);
                data[offset++] = (byte)(subid & 0x7F);
            }
            else if (subid < 0x200000u)
            { // Size = 3
                data[offset++] = (byte)((subid >> 14) | 0x80);
                data[offset++] = (byte)(((subid >> 7) & 0x7F) | 0x80);
                data[offset++] = (byte)(subid & 0x7F);
            }
            else if (subid < 0x10000000u)
            { // Size = 4
                data[offset++] = (byte)((subid >> 21) | 0x80);
                data[offset++] = (byte)(((subid >> 14) & 0x7F) | 0x80);
                data[offset++] = (byte)(((subid >> 7) & 0x7F) | 0x80);
                data[offset++] = (byte)(subid & 0x7F);
            }
            else
            { // Size = 5
                data[offset++] = (byte)((subid >> 28) | 0x80);
                data[offset++] = (byte)(((subid >> 21) & 0x7F) | 0x80);
                data[offset++] = (byte)(((subid >> 14) & 0x7F) | 0x80);
                data[offset++] = (byte)(((subid >> 7) & 0x7F) | 0x80);
                data[offset++] = (byte)(subid & 0x7F);
            }
            return offset;
        }
    }
}
