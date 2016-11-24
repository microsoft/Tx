
namespace Tx.Network.Snmp
{
    using Asn1Types;
    using System;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Class to Encode\Decode SNMP data
    /// </summary>
    internal static class SnmpEncodeDecoder
    {
        /// <summary>
        /// The header length offset
        /// </summary>
        private const int SnmpV2MessageHeaderLength = 32;

        /// <summary>
        /// To Asn.1/Snmp Encoded byte array.
        /// </summary>
        /// <param name="snmpPacket">The SNMP packet.</param>
        /// <returns>Asn.1 encoded byte array</returns>
        /// <exception cref="System.ArgumentNullException">snmpPacket</exception>
        public static byte[] ToSnmpEncodedByteArray(this SnmpDatagramV2C snmpPacket)
        {
            byte[] dataBytes = new byte[8194];
            int length = dataBytes.EncodeVarBinds(0, snmpPacket.VarBinds);
            Array.Resize(ref dataBytes, length);

            int headerLength = SnmpV2MessageHeaderLength + snmpPacket.Header.Community.Length;
            byte[] headerBytes = new byte[headerLength];

            int offset = 0;

            //Encode version
            offset = headerBytes.EncodeInteger(offset, (int)snmpPacket.Header.Version);

            //Encode Community String
            offset = headerBytes.EncodeOctetString(offset, snmpPacket.Header.Community);

            //Encode PDU Type
            offset = headerBytes.EncodeClassConstructType(offset, Asn1Class.ContextSpecific, ConstructType.Constructed, (byte)snmpPacket.PduType);

            int commonPduControlFieldLength =
                1 //pduType
                + snmpPacket.RequestId.GetIntegerLength()
                + ((int)snmpPacket.ErrorStatus).GetIntegerLength()
                + snmpPacket.ErrorIndex.GetIntegerLength()
                + length; //length of varbind values


            //Encode PDU length
            offset = headerBytes.EncodeLength(offset, commonPduControlFieldLength);

            //Encode RequestId
            offset = headerBytes.EncodeInteger(offset, snmpPacket.RequestId);

            //Encode ErrorStatus
            offset = headerBytes.EncodeInteger(offset, (int)snmpPacket.ErrorStatus);

            //Encode ErrorIndex
            offset = headerBytes.EncodeInteger(offset, snmpPacket.ErrorIndex);

            //Encode VarBinds Length
            offset = headerBytes.EncodeClassConstructType(offset, Asn1Class.Universal, ConstructType.Constructed, (byte)Asn1Tag.Sequence);
            offset = headerBytes.EncodeLength(offset, length);

            byte[] allBytes = new byte[6];
            int newOffset = 0;
            newOffset = allBytes.EncodeClassConstructType(newOffset, Asn1Class.Universal, ConstructType.Constructed, (byte)Asn1Tag.Sequence);
            newOffset = allBytes.EncodeLength(newOffset, offset + length);
            //Resize and append varbinds to header
            Array.Resize(ref allBytes, newOffset + offset + length);
            Array.Copy(headerBytes, 0, allBytes, newOffset, offset);
            Array.Copy(dataBytes, 0, allBytes, newOffset + offset, length);

            return allBytes;
        }

        /// <summary>
        /// Converts the Asn.1 encoded byte array to SNMP Datagram.
        /// </summary>
        /// <param name="byteSegment">The asn.1 encoded bytes.</param>
        /// <param name="timestamp">The timestamp when the packet has been received.</param>
        /// <param name="sourceIpAddress">The source IP address of the packet.</param>
        /// <returns>
        /// SnmpPacket
        /// </returns>
        /// <exception cref="System.ArgumentNullException">bytes</exception>
        /// <exception cref="System.IO.InvalidDataException">Snmp Version V3 not supported</exception>
        public static SnmpDatagram ToSnmpDatagram(
            this ArraySegment<byte> byteSegment,
            DateTimeOffset timestamp,
            string sourceIpAddress)
        {
            var bytes = byteSegment.Array;
            if(bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("byteSegment");
            }

            int offset = byteSegment.Offset;
            int length;
            offset = bytes.NextValueLength(offset, -1, -1, -1, out length);
            offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
            SnmpVersion snmpVersion = (SnmpVersion)bytes.ReadInteger(offset, length);
            offset += length;

            if (snmpVersion == SnmpVersion.V3)
            {
                throw new InvalidDataException("Snmp Version V3 not supported");
            }

            offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.OctetString, out length);
            string community = bytes.ReadOctetString(offset, length);
            offset += length;

            PduType pduType = (PduType)(bytes[offset++] & 0x1F);
            offset = bytes.ReadLength(offset, out length);
            if (snmpVersion == SnmpVersion.V1 && pduType== PduType.Trap)
            {
                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.ObjectIdentifier, out length);
                ObjectIdentifier oid = new ObjectIdentifier(bytes.ReadOids(offset, length));
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1SnmpTag.IpAddress, out length);
                IPAddress ipAddress = bytes.ReadIpAddress(offset);
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
                GenericTrap genericTrap = (GenericTrap)bytes.ReadInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
                int specificTrap = bytes.ReadInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1SnmpTag.TimeTicks, out length);
                uint timeStamp = bytes.ReadUnsignedInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, (int)Asn1Class.Universal, (int)ConstructType.Constructed, (int)Asn1Tag.Sequence, out length);
                VarBind[] varBinds = bytes.ReadVarBinds(offset, length);

                return new SnmpDatagramV1(
                    timestamp,
                    sourceIpAddress,
                    new SnmpHeader(snmpVersion, community), 
                    varBinds);
            }
            else
            {
                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
                int requestId = bytes.ReadInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
                SnmpErrorStatus errorStatus = (SnmpErrorStatus)bytes.ReadInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, -1, -1, (int)Asn1Tag.Integer, out length);
                int errorIndex = bytes.ReadInteger(offset, length);
                offset += length;

                offset = bytes.NextValueLength(offset, (int)Asn1Class.Universal, (int)ConstructType.Constructed, (int)Asn1Tag.Sequence, out length);
                VarBind[] varBinds = bytes.ReadVarBinds(offset, length);

                return new SnmpDatagramV2C(
                    timestamp, 
                    sourceIpAddress, 
                    new SnmpHeader(snmpVersion, community), 
                    varBinds,
                    pduType,
                    requestId,
                    errorStatus,
                    errorIndex);
            }
        }

        /// <summary>
        /// Nexts the length of the value.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="classValidations">The class validations.</param>
        /// <param name="constructValidations">The construct validations.</param>
        /// <param name="typeValidations">The type validations.</param>
        /// <param name="length">The length.</param>
        /// <returns>int [value to next offset]</returns>
        /// <exception cref="DataMisalignedException">
        /// Data Malformated/Expected Asn1 Class tag is:  + ((Asn1Class)classValidations).ToString()
        /// or
        /// Data Malformated/Expected ConstructType tag is:  + ((ConstructType)constructValidations).ToString()
        /// or
        /// Data Malformated/Expected Asn1 tag is:  + ((Asn1Tag)typeValidations).ToString()
        /// or
        /// Data Malformated/Expected Asn1Snmp tag is:  + ((Asn1SnmpTag)typeValidations).ToString()
        /// </exception>
        private static int NextValueLength(this byte[] bytes, int offset, int classValidations, int constructValidations, int typeValidations, out int length)
        {
            var type = bytes[offset++].DecodeToClassConstructType();
            if (classValidations != -1 && type.Asn1ClassType != (Asn1Class)classValidations)
            {
                throw new DataMisalignedException("Data Malformated/Expected Asn1 Class tag is: " + ((Asn1Class)classValidations).ToString());
            }

            if (constructValidations != -1 && type.Asn1ConstructType != (ConstructType)constructValidations)
            {
                throw new DataMisalignedException("Data Malformated/Expected ConstructType tag is: " + ((ConstructType)constructValidations).ToString());
            }

            if (type.Asn1TagType != Asn1Tag.NotAsn1Data)
            {
                if (typeValidations != -1 && type.Asn1TagType != (Asn1Tag)typeValidations)
                {
                    throw new DataMisalignedException("Data Malformated/Expected Asn1 tag is: " + ((Asn1Tag)typeValidations).ToString());
                }
            }
            else
            {
                if (typeValidations != -1 && type.Asn1SnmpTagType != (Asn1SnmpTag)typeValidations)
                {
                    throw new DataMisalignedException("Data Malformated/Expected Asn1Snmp tag is: " + ((Asn1SnmpTag)typeValidations).ToString());
                }
            }

            return bytes.ReadLength(offset, out length);
        }
    }
}
