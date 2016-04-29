
namespace Tx.Network.Snmp
{
    using System.Collections.Generic;

    /// <summary>
    /// Protocol definition class for simple data representation for SnmpTrapV2 data.
    /// This also works fine for V1 Version, get the Enterprise name from TrapId.
    /// </summary>
    public class SnmpSimpleTrapV2C : SnmpSimpleDatagram
    {
        /// <summary>
        /// The enterprise oid
        /// </summary>
        private readonly static string trapOid = "1.3.6.1.6.3.1.1.4.1.0";

        /// <summary>
        /// The system up time oid
        /// </summary>
        private readonly static string sysUpTimeOid = "1.3.6.1.2.1.1.3.0";

        /// <summary>
        /// Gets the SysUpTime which represents in 100th of a second.
        /// </summary>
        /// <value>
        /// SysUpTime which represents in 100th of a second.
        /// </value>
        public uint SysUpTime { get; private set; }

        /// <summary>
        /// Gets the trap Oid String.
        /// </summary>
        /// <value>
        /// The trap oid.
        /// </value>
        public string TrapOid { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpTrapV2C"/> class.
        /// </summary>
        /// <param name="bytes">The snmp encoded bytes.</param>
        public SnmpSimpleTrapV2C(byte[] bytes)
            :base(bytes)
        {
            TrapOid = string.Empty;
            SysUpTime = 0;
            KeyValuePair<string,object> varBind;
            if (base.SearchFirstSubOidWith(sysUpTimeOid, out varBind))
            {
                SysUpTime = (uint)varBind.Value;
            }

            if (base.SearchFirstSubOidWith(trapOid, out varBind))
            {
                TrapOid = ((ObjectIdentifier)varBind.Value).ToString();
            }
        }
    }
}
