namespace Tx.Network.Snmp
{
    /// <summary>
    /// Protocol definition class for SnmpTrapV2 data.
    /// This also works fine for V1 Version, get the Enterprise name from TrapId.
    /// </summary>
    public class SnmpTrapV2C : SnmpDatagram
    {
        /// <summary>
        /// The enterprise oid
        /// </summary>
        private readonly static ObjectIdentifier trapOid = new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0");

        /// <summary>
        /// The system up time oid
        /// </summary>
        private readonly static ObjectIdentifier sysUpTimeOid = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");

        /// <summary>
        /// Gets the time stamp.
        /// </summary>
        /// <value>
        /// The time stamp.
        /// </value>
        public uint SysUpTime { get; private set; }

        /// <summary>
        /// Gets the trap object identifier.
        /// </summary>
        /// <value>
        /// The trap object identifier.
        /// </value>
        public ObjectIdentifier TrapOid { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV2C"/> class.
        /// </summary>
        /// <param name="bytes">The snmp encoded bytes.</param>
        public SnmpTrapV2C(byte[] bytes)
            : base(bytes.ToSnmpPacket())
        {
            TrapOid = default(ObjectIdentifier);
            SysUpTime = 0;
            VarBind varBind;
            if (PDU.SearchFirstSubOidWith(sysUpTimeOid, out varBind) && varBind.Asn1TypeInfo.Asn1SnmpTagType == Asn1SnmpTag.TimeTicks)
            {
                SysUpTime = (uint)varBind.Value;
            }

            if (PDU.SearchFirstSubOidWith(trapOid, out varBind) && varBind.Asn1TypeInfo.Asn1TagType == Asn1Tag.ObjectIdentifier)
            {
                TrapOid = varBind.Oid;
            }
        }
    }
}
