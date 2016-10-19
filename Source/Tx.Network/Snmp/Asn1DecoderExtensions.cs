namespace Tx.Network.Snmp
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Internal class that provides extension methods for Asn1 decoder
    /// </summary>
    internal static class Asn1DecoderExtensions
    {
        /// <summary>
        /// Decodes the type of to class construct.
        /// </summary>
        /// <param name="byteToDecode">The byte to decode.</param>
        /// <returns></returns>
        public static Asn1TagInfo DecodeToClassConstructType(this byte byteToDecode)
        {
            return new Asn1TagInfo((byteToDecode & 0xC0) >> 6, (byteToDecode & 0x20) >> 5, byteToDecode & 0x1F);
        }

        /// <summary>
        /// Reads the unsigned integer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>uint</returns>
        public static uint ReadUnsignedInteger(this byte[] bytes, int offset, int length)
        {
            uint value = 0;
            int endOfContentIndex = offset + length;
            for (int i = offset; i < endOfContentIndex; i++)
            {
                value = (value << 8) | bytes[i];
            }

            return value;
        }

        /// <summary>
        /// Reads the integer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>int</returns>
        public static int ReadInteger(this byte[] bytes, int offset, int length)
        {
            int value = 0;
            int endOfContentIndex = offset + length;
            for (int i = offset; i < endOfContentIndex; i++)
            {
                value = (value << 8) | bytes[i];
            }

            return value;
        }

        /// <summary>
        /// Reads the long integer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>long</returns>
        public static long ReadLongInteger(this byte[] bytes, int offset, int length)
        {
            long value = 0;
            int endOfContentIndex = offset + length;
            for (int i = offset; i < endOfContentIndex; i++)
            {
                value = (value << 8) | bytes[i];
            }

            return value;
        }

        /// <summary>
        /// Reads the unsigned long integer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>ulong</returns>
        private static ulong ReadUnsignedLong(byte[] bytes, int offset, int length)
        {
            ulong value = 0;
            int endOfContentIndex = offset + length;
            for (int i = offset; i < endOfContentIndex; i++)
            {
                value = (value << 8) | bytes[i];
            }

            return value;
        }

        /// <summary>
        /// Reads the octet string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>string</returns>
        internal static string ReadOctetString(this byte[] bytes, int offset, int length)
        {
            var stringValue = Encoding.UTF8.GetString(bytes, offset, length);

            // If there are non-printable char, it has hex value.
            if (stringValue.Any(char.IsControl))
            {
                // hex value
                stringValue = BitConverter.ToString(bytes, offset, length);
            }

            return stringValue;
        }

        /// <summary>
        /// Reads the ip address.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="startOffset">The start offset.</param>
        /// <returns>IPAddress</returns>
        private static System.Net.IPAddress ReadIPAddress(byte[] bytes, int startOffset)
        {
            return new System.Net.IPAddress(new byte[]{bytes[startOffset], bytes[startOffset +1], bytes[startOffset+2],bytes[startOffset+3]});
        }

        /// <summary>
        /// Reads the variable binds.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bytesLength">Length of the bytes.</param>
        /// <returns>
        /// VarBinds
        /// </returns>
        /// <exception cref="System.DataMisalignedException">Bad Data/Mulformated Asn1/Snmp data</exception>
        /// <exception cref="InvalidOperationException">Malformed datagram/Out of sequemce data</exception>
        public static VarBind[] ReadVarBinds(this byte[] bytes, int offset, int bytesLength)
        {
            VarBind[] values = new VarBind[100];
            int varbindCount = 0;
            bytesLength += offset;

            while (offset < bytesLength)
            {
                Asn1TagInfo cct = bytes[offset++].DecodeToClassConstructType();
                int length = 0;
                offset = ReadLength(bytes, offset, out length);
  
                if (ConstructType.Primitive != cct.Asn1ConstructType)
                {
                    continue;
                }

                if (cct.Asn1TagType == Asn1Tag.NotAsn1Data || cct.Asn1TagType != Asn1Tag.ObjectIdentifier)
                {
                    throw new DataMisalignedException("Bad Data/Mulformated Asn1/Snmp data");
                }

                uint[] key = ReadOids(bytes, offset, length);
                offset += length;

                // Get [TLV] type, length, value
                cct = bytes[offset++].DecodeToClassConstructType();
                offset = ReadLength(bytes, offset, out length);
                object value;
                offset = GetVarBindValue(bytes, offset, length, cct, out value);

                //Make varbind
                MakeVarBinds(ref values, key, value, cct, varbindCount++);
            }

            if (varbindCount != values.Length)
            {
                Array.Resize(ref values, varbindCount);
            }

            return values;
        }

        /// <summary>
        /// Gets the variable bind value.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="tagInfo">The tag information.</param>
        /// <param name="value">The value.</param>
        /// <returns>offset int</returns>
        private static int GetVarBindValue(byte[] bytes, int offset, int length, Asn1TagInfo tagInfo, out object value)
        {
            if (tagInfo.Asn1TagType == Asn1Tag.NotAsn1Data)
            {
                switch (tagInfo.Asn1SnmpTagType)
                {
                    case Asn1SnmpTag.IpAddress:
                        {
                            value = ReadIPAddress(bytes, offset);
                            break;
                        }

                    case Asn1SnmpTag.Counter:
                        {
                            value = ReadUnsignedInteger(bytes, offset, length);
                            break;
                        }

                    case Asn1SnmpTag.Counter64:
                        {
                            value = ReadUnsignedLong(bytes, offset, length);
                            break;
                        }

                    case Asn1SnmpTag.Gauge:
                        {
                            value = ReadUnsignedInteger(bytes, offset, length);
                            break;
                        }

                    case Asn1SnmpTag.UInt32:
                        {
                            value = ReadUnsignedInteger(bytes, offset, length);
                            break;
                        }

                    case Asn1SnmpTag.TimeTicks:
                        {
                            value = ReadUnsignedInteger(bytes, offset, length);
                            break;
                        }

                    default:
                        {
                            value = "NYI: " + tagInfo.Asn1SnmpTagType.ToString();
                            break;
                        }
                }
            }
            else
            {
                switch (tagInfo.Asn1TagType)
                {
                    case Asn1Tag.Null:
                        {
                            value = null;
                            break;
                        }

                    case Asn1Tag.Integer:
                        {
                            value = ReadLongInteger(bytes, offset, length);
                            break;
                        }

                    case Asn1Tag.OctetString:
                        {
                            value = ReadOctetString(bytes, offset, length);
                            break;
                        }

                    case Asn1Tag.ObjectIdentifier:
                        {
                            value = new ObjectIdentifier(bytes.ReadOids(offset, length));
                            break;
                        }

                    default:
                        {
                            value = "NYI: " + tagInfo.Asn1TagType.ToString();
                            break;
                        }
                }
            }

            return offset + length;
        }

        /// <summary>
        /// Makes the variable binds.
        /// </summary>
        /// <param name="varBinds">Varbind value.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The asn1 tag.</param>
        /// <param name="varbindCount">The varbind count.</param>
        private static void MakeVarBinds(ref VarBind[] varBinds, uint[] key, object value, Asn1TagInfo tag, int varbindCount)
        {
            int length = varBinds.Length;
            if (varbindCount >= length)
            {
                Array.Resize(ref varBinds, length + 100);
            }

            varBinds[varbindCount] = new VarBind(new ObjectIdentifier(key), value, tag);
        }

        /// <summary>
        /// Reads the oids.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>uint array</returns>
        public static uint[] ReadOids(this byte[] bytes, int offset, int length)
        {
            uint subId = 0;
            uint[] oids = new uint[length];
            int count = 2;
            int endOfContentIndex = offset + length - 1;
            offset = DecodeSubID(bytes, offset, out subId);

            if (subId < 40)
            {
                oids[0] = 0;
                oids[1] = subId;
            }
            else if (subId < 80)
            {
                oids[0] = 1;
                oids[1] = (subId - 40);
            }
            else
            {
                oids[0] = 2;
                oids[1] = (subId - 80);
            }

            while (offset <= endOfContentIndex)
            {
                offset = DecodeSubID(bytes, offset, out subId);
                if (length > count)
                {
                    oids[count++] = subId;
                }
                else
                {
                    Array.Resize(ref oids, count + 1);
                    oids[count++] = subId;
                }
            }

            if (oids.Length != count)
            {
                Array.Resize(ref oids, count);
            }

            return oids;
        }

        /// <summary>
        /// Decodes the sub identifier.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="subID">The sub identifier.</param>
        /// <returns></returns>
        private static int DecodeSubID(byte[] data, int offset, out uint subID)
        {
            subID = 0;
            byte b;
            do
            {
                b = data[offset++];
                subID <<= 7;
                subID |= (uint)(b & (byte)0x7F);
            } while ((b & 0x80) == 0x80);

            return offset;
        }

        /// <summary>
        /// Reads the length.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Indefinite lengths are not implemented.</exception>
        internal static int ReadLength(this byte[] data, int offset, out int value)
        {
            int length = data[offset++];
            if (length == 0x80)
                throw new NotSupportedException("Indefinite lengths are not implemented.");

            if (length <= 127)
            {
                value = length;
                return offset;
            }

            int numLengthBytes = (length & 0x7F);
            value = ReadInteger(data, offset, numLengthBytes);

            return offset + numLengthBytes;
        }
    }
}
