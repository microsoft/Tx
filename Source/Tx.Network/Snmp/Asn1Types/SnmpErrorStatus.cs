namespace Tx.Network.Snmp
{
    public enum SnmpErrorStatus
    {
        /// <summary>
        /// SNMPv1/2c: Sent on requests packets.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// SNMPv1/2c: The request would take a response packet that would be to large.
        /// </summary>
        TooBig = 1,

        /// <summary>
        /// SNMPv1/2c: No such OID exists. For V2 this will be an exception in the value of the VarBind.
        /// </summary>
        NoSuchName = 2,

        /// <summary>
        /// SNMPv1/2c: The valid is in an incorrect format.
        /// </summary>
        BadValue = 3,

        /// <summary>
        /// SNMPv1/2c: Only valid for sets which aren't currently supported by this library.
        /// </summary>
        ReadOnly = 4,

        /// <summary>
        /// SNMPv1/2c: A general error occurred.
        /// </summary>
        GenErr = 5,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        NoAccess = 6,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        WrongType = 7,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        WrongLength = 8,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        WrongEncoding = 9,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        WrongValue = 10,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        NoCreation = 11,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        InconsistentValue = 12,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        ResourceUnavailable = 13,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        CommitFailed = 14,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        UndoFailed = 15,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        AuthorizationError = 16,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        NotWritable = 17,

        /// <summary>
        /// SNMPv2 Errors only.
        /// </summary>
        InconsistentName = 18
    };
}
