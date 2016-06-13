
namespace Tx.Network.Snmp
{
    using System.Net;
    using Tx.Network.Snmp.Asn1Types;

    /// <summary>
    /// Struct to provide decoded snmp data
    /// </summary>
    public struct SnmpDatagram
    {
        /// <summary>
        /// Gets the header.
        /// </summary>
        public readonly SnmpHeader Header;

        /// <summary>
        /// Gets the V2c pdu.
        /// </summary>
        public readonly SnmpV2cPDU PduV2c;

        /// <summary>
        /// Gets the V2c pdu.
        /// </summary>
        public readonly SnmpV1PDU PduV1;

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
            PduV2c = new SnmpV2cPDU(pduType, requestId, errorStatus, errorIndex, varBinds);
            PduV1 = default(SnmpV1PDU);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="snmpVersion">The SNMP version.</param>
        /// <param name="community">The community.</param>
        /// <param name="enterprise">The enterprise.</param>
        /// <param name="agentAddress">The agent address.</param>
        /// <param name="genericV1Trap">The generic v1 trap.</param>
        /// <param name="specificTrap">The specific trap.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="varBinds">The variable binds.</param>
        public SnmpDatagram(PduType pduType, SnmpVersion snmpVersion, string community, ObjectIdentifier enterprise, IPAddress agentAddress, GenericTrap genericV1Trap, int specificTrap, uint timeStamp, VarBind[] varBinds)
        {
            Header = new SnmpHeader(snmpVersion, community);
            PduV1 = new SnmpV1PDU(pduType, enterprise, agentAddress, genericV1Trap, specificTrap, timeStamp, varBinds);
            PduV2c = default(SnmpV2cPDU);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// </summary>
        /// <param name="snmpHeader">The SNMP header.</param>
        /// <param name="snmpPDU">The SNMP pdu.</param>
        public SnmpDatagram(SnmpHeader snmpHeader, SnmpV2cPDU snmpPDU)
        {
            Header = snmpHeader;
            PduV2c = snmpPDU;
            PduV1 = default(SnmpV1PDU);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// </summary>
        /// <param name="snmpHeader">The SNMP header.</param>
        /// <param name="snmpPDU">The SNMP pdu.</param>
        public SnmpDatagram(SnmpHeader snmpHeader, SnmpV1PDU snmpPDU)
        {
            Header = snmpHeader;
            PduV1 = snmpPDU;
            PduV2c = default(SnmpV2cPDU);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpDatagram"/> class.
        /// Copy construtor
        /// </summary>
        /// <param name="snmpPacket">The SNMP packet.</param>
        public SnmpDatagram(SnmpDatagram snmpPacket)
        {
            Header = snmpPacket.Header;
            PduV1 = snmpPacket.PduV1;
            PduV2c = snmpPacket.PduV2c;
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
            if (Header.Version == SnmpVersion.V2C)
            {
                sb.Append("request-id: ");
                sb.Append(PduV2c.RequestId);
                sb.AppendLine();
                sb.Append("error-status: ");
                sb.Append(PduV2c.ErrorStatus);
                sb.AppendLine();
                sb.Append("error-index: ");
                sb.Append(PduV2c.ErrorIndex);
            }
            else
            {
                sb.Append("Enterprise: ");
                sb.Append(PduV1.Enterprise.ToString());
                sb.AppendLine();
                sb.Append("AgentAddress: ");
                sb.Append(PduV1.AgentAddress);
                sb.AppendLine();
                sb.Append("GenericV1Trap: ");
                sb.Append(PduV1.GenericV1Trap.ToString());
                sb.AppendLine();
                sb.Append("SpecificTrap: ");
                sb.Append(PduV1.SpecificTrap.ToString());
                sb.AppendLine();
                sb.Append("TimeStamp: ");
                sb.Append(PduV1.TimeStamp.ToString());
            }

            sb.AppendLine();
            sb.AppendLine("VarBinds:-");
            var vbs = (Header.Version == SnmpVersion.V2C) ?PduV2c.VarBinds: PduV1.VarBinds;
            foreach (var vb in vbs)
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
