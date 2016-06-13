
namespace Tx.Network.Snmp
{
    using System.IO;

    /// <summary>
    /// Protocol definition class for SnmpTrap V1 data.
    /// </summary>
    public struct SnmpTrapV1
    {
        /// <summary>
        /// The pdu for V2C Trap.
        /// </summary>
        public readonly SnmpV1PDU PduV1;

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
        /// Initializes a new instance of the <see cref="SnmpTrapV1"/> class.
        /// </summary>
        /// <param name="snmpDatagram">The snmp datagram.</param>
        public SnmpTrapV1(SnmpDatagram snmpDatagram)
        {
            if (snmpDatagram.Header.Version != SnmpVersion.V1 && snmpDatagram.PduV1.PduType != PduType.Trap)
            {
                throw new InvalidDataException("Not a Valid V1 Trap");
            }

            PduV1 = snmpDatagram.PduV1;
            Header = snmpDatagram.Header;
            TrapOid = PduV1.Enterprise;
            SysUpTime = PduV1.TimeStamp;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV1"/> struct.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public SnmpTrapV1(byte[] bytes):this(bytes.ToSnmpDatagram())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV1"/> struct.
        /// </summary>
        /// <param name="ipPacket">The ip packet.</param>
        public SnmpTrapV1(IpPacket ipPacket) : this(ipPacket.PacketData)
        {
        }
    }
}
