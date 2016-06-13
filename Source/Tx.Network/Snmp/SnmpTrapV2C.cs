
namespace Tx.Network.Snmp
{
    using System.IO;

    /// <summary>
    /// Protocol definition class for SnmpTrap V2c data.
    /// </summary>
    public struct SnmpTrapV2C
    {
        /// <summary>
        /// The pdu for V2C Trap.
        /// </summary>
        public readonly SnmpV2cPDU PduV2c;

        /// <summary>
        /// The Snmp header
        /// </summary>
        public readonly SnmpHeader Header;

        /// <summary>
        /// Gets the SysUpTime which represents in 100th of a second.
        /// </summary>
        /// <value>
        /// SysUpTime which represents in 100th of a second.
        /// </value>
        public readonly uint SysUpTime;

        /// <summary>
        /// Gets the trap object identifier.
        /// </summary>
        /// <value>
        /// The trap object identifier.
        /// </value>
        public readonly ObjectIdentifier TrapOid;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV2C"/> class.
        /// </summary>
        /// <param name="snmpDatagram">The snmp datagram.</param>
        public SnmpTrapV2C(SnmpDatagram snmpDatagram)
        {
            if(snmpDatagram.Header.Version != SnmpVersion.V2C || snmpDatagram.PduV2c.PduType == PduType.Trap)
            {
                throw new InvalidDataException("Not a Valid V2c Trap");
            }

            ObjectIdentifier trapOid = new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0");
            ObjectIdentifier sysUpTimeOid = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");

            PduV2c = snmpDatagram.PduV2c;
            Header = snmpDatagram.Header;
            TrapOid = default(ObjectIdentifier);
            SysUpTime = 0;
            VarBind varBind;
            if (PduV2c.VarBinds.SearchFirstSubOidWith(sysUpTimeOid, out varBind) && varBind.Asn1TypeInfo.Asn1SnmpTagType == Asn1SnmpTag.TimeTicks)
            {
                SysUpTime = (uint)varBind.Value;
            }

            if (PduV2c.VarBinds.SearchFirstSubOidWith(trapOid, out varBind) && varBind.Asn1TypeInfo.Asn1TagType == Asn1Tag.ObjectIdentifier)
            {
                TrapOid = (ObjectIdentifier)varBind.Value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV2C"/> struct.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public SnmpTrapV2C(byte[] bytes):this(bytes.ToSnmpDatagram())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV2C"/> struct.
        /// </summary>
        /// <param name="ipPacket">The ip packet.</param>
        public SnmpTrapV2C(IpPacket ipPacket) : this(ipPacket.PacketData)
        {
        }

    }
}
