namespace Tx.Network.Snmp
{
    public enum PduType
    {
        /// <summary>
        /// The get request
        /// </summary>
        GetRequest = 0,

        /// <summary>
        /// The get next request
        /// </summary>
        GetNextRequest = 1,

        /// <summary>
        /// The response
        /// </summary>
        Response = 2,

        /// <summary>
        /// The set request
        /// </summary>
        SetRequest = 3,

        /// <summary>
        /// The trap
        /// This existed in SNMPv1 and was obsoleted in SNMPv2
        /// </summary>
        Trap = 4,

        /// <summary>
        /// The get bulk request
        /// </summary>
        GetBulkRequest = 5,

        /// <summary>
        /// The inform request
        /// </summary>
        InformRequest = 6,

        /// <summary>
        /// The SNMPV2 trap
        /// </summary>
        SNMPv2Trap = 7,

        /// <summary>
        /// The report
        /// </summary>
        Report = 8
    }
}
