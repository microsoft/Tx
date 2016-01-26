namespace Tx.Network.Snmp
{
    /// <summary>
    /// Class to provide decoded snmp data
    /// </summary>
    public class SnmpDatagram
    {
        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public SnmpHeader Header { get; private set; }

        /// <summary>
        /// Gets the pdu.
        /// </summary>
        /// <value>
        /// The pdu.
        /// </value>
        public SnmpPDU PDU { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="snmpVersion">The SNMP version.</param>
        /// <param name="community">The community.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="errorStatus">The error status.</param>
        /// <param name="errorIndex">Index of the error.</param>
        /// <param name="varBinds">The variable binds.</param>
        public SnmpDatagram(PduType pduType, SnmpVersion snmpVersion, string community, int requestId, SnmpErrorStatus errorStatus, int errorIndex, VarBind[] varBinds)
        {
            Header = new SnmpHeader(snmpVersion, community);
            PDU = new SnmpPDU(pduType, requestId, errorStatus, errorIndex, varBinds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// </summary>
        /// <param name="snmpHeader">The SNMP header.</param>
        /// <param name="snmpPDU">The SNMP pdu.</param>
        public SnmpDatagram(SnmpHeader snmpHeader, SnmpPDU snmpPDU)
        {
            Header = snmpHeader;
            PDU = snmpPDU;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// Copy construtor
        /// </summary>
        /// <param name="snmpPacket">The SNMP packet.</param>
        public SnmpDatagram(SnmpDatagram snmpPacket)
        {
            Header = snmpPacket.Header;
            PDU = snmpPacket.PDU;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("version: ");
            sb.Append(Header.Version);
            sb.AppendLine();
            sb.Append("community: ");
            sb.AppendLine(Header.Community);
            sb.Append("request-id: ");
            sb.Append(PDU.RequestId);
            sb.AppendLine();
            sb.Append("error-status: ");
            sb.Append(PDU.ErrorStatus);
            sb.AppendLine();
            sb.Append("error-index: ");
            sb.Append(PDU.ErrorIndex);
            sb.AppendLine();
            sb.AppendLine("VarBinds:-");
            foreach (var vb in PDU.VarBinds)
            {
                sb.Append(vb.Oid);
                sb.Append("=");
                if (vb.Value == null)
                    sb.AppendLine("(null)");
                else
                    sb.AppendLine(vb.Value.ToString()
                        + " -Class:" + vb.Asn1TypeInfo.Asn1ClassType.ToString()
                        + " -Construct:" + vb.Asn1TypeInfo.Asn1ConstructType.ToString()
                        + ((vb.Asn1TypeInfo.Asn1TagType != Asn1Tag.NotAsn1Data)? " -Asn1Tag:" + vb.Asn1TypeInfo.Asn1ClassType.ToString():
                        " -Asn1SnmpTag:" + vb.Asn1TypeInfo.Asn1SnmpTagType.ToString()));
            }

            return sb.ToString();
        }
    }
}
