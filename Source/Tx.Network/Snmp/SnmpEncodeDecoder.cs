
namespace Tx.Network.Snmp
{
    using System;
    using System.IO;

    /// <summary>
    /// Class to Encode\Decode SNMP data
    /// </summary>
    public static class SnmpEncodeDecoder
    {
        /// <summary>
        /// The common pdu control field length
        /// </summary>
        private const int CommonPduControlFieldLength = 14;

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
        public static byte[] ToSnmpEncodedByteArray(this SnmpDatagram snmpPacket)
        {
            if (snmpPacket == null)
            {
                throw new ArgumentNullException("snmpPacket");
            }

            if (snmpPacket.Header.Version == SnmpVersion.V3)
            {
                throw new InvalidDataException("Snmp Version V3 not supported");
            }

            byte[] dataBytes = new byte[8194];
            int length = dataBytes.EncodeVarBinds(0, snmpPacket.PDU.VarBinds);
            Array.Resize(ref dataBytes, length);

            int headerLength = SnmpV2MessageHeaderLength + snmpPacket.Header.Community.Length;
            byte[] headerBytes = new byte[headerLength];

            int offset = 0;

            //Encode version
            offset = headerBytes.EncodeInteger(offset, (int)snmpPacket.Header.Version);

            //Encode Community String
            offset = headerBytes.EncodeOctetString(offset, snmpPacket.Header.Community);

            //Encode PDU Type
            offset = headerBytes.EncodeClassConstructType(offset, Asn1Class.ContextSpecific, ConstructType.Constructed, (byte)snmpPacket.PDU.PduType);

            //Encode PDU length
            offset = headerBytes.EncodeLength(offset, CommonPduControlFieldLength + length);

            //Encode RequestId
            offset = headerBytes.EncodeInteger(offset, snmpPacket.PDU.RequestId);

            //Encode ErrorStatus
            offset = headerBytes.EncodeInteger(offset, (int)snmpPacket.PDU.ErrorStatus);

            //Encode ErrorIndex
            offset = headerBytes.EncodeInteger(offset, snmpPacket.PDU.ErrorIndex);

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
        /// Converts the Asn.1 byte array to SNMP packet.
        /// </summary>
        /// <param name="bytes">The asn.1 encoded bytes.</param>
        /// <returns>
        /// SnmpPacket
        /// </returns>
        /// <exception cref="System.ArgumentNullException">bytes</exception>
        /// <exception cref="System.IO.InvalidDataException">Snmp Version V3 not supported</exception>
        /// <exception cref="System.NotImplementedException">SNMP v1 traps are not yet implemented</exception>
        public static SnmpDatagram ToSnmpPacket(this byte[] bytes)
        {
            if(bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("bytes");
            }

            int offset = 0;
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
            if (pduType == PduType.Trap)
            {
                throw new NotImplementedException("SNMP v1 traps are not yet implemented");
            }

            offset = bytes.ReadLength(offset, out length);

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

            return new SnmpDatagram(new SnmpHeader(snmpVersion, community), new SnmpPDU(pduType, varBinds, requestId, errorStatus, errorIndex));
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
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// Expected Ans1 Class tag is:  + ((Asn1Class)classValidations).ToString()
        /// or
        /// Expected ConstructType tag is:  + ((ConstructType)constructValidations).ToString()
        /// or
        /// Expected Ans1 tag is:  + ((Asn1Tag)typeValidations).ToString()
        /// </exception>
        private static int NextValueLength(this byte[] bytes, int offset, int classValidations, int constructValidations, int typeValidations, out int length)
        {
            var type = bytes[offset++].DecodeToClassConstructType();
            if (classValidations != -1 && type.Asn1ClassType != (Asn1Class)classValidations)
            {
                throw new InvalidDataException("Expected Ans1 Class tag is: " + ((Asn1Class)classValidations).ToString());
            }

            if (constructValidations != -1 && type.Asn1ConstructType != (ConstructType)constructValidations)
            {
                throw new InvalidDataException("Expected ConstructType tag is: " + ((ConstructType)constructValidations).ToString());
            }

            if (typeValidations != -1 && type.Asn1TagType != (Asn1Tag)typeValidations)
            {
                throw new InvalidDataException("Expected Ans1 tag is: " + ((Asn1Tag)typeValidations).ToString());
            }

            return bytes.ReadLength(offset, out length);
        }
    }
}
