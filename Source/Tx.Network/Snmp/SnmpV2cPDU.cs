namespace Tx.Network.Snmp
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Class to represent PDU for Snmp V2c
    /// </summary>
    public struct SnmpV2cPDU
    {
        /// <summary>
        /// Gets the type of the pdu.
        /// </summary>
        /// <value>
        /// The type of the pdu.
        /// </value>
        public readonly PduType PduType;

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public readonly int RequestId;

        /// <summary>
        /// Gets the error status.
        /// </summary>
        /// <value>
        /// The error status.
        /// </value>
        public readonly SnmpErrorStatus ErrorStatus;

        /// <summary>
        /// Gets the index of the error.
        /// </summary>
        /// <value>
        /// The index of the error.
        /// </value>
        public readonly int ErrorIndex;

        /// <summary>
        /// Gets the variable binds.
        /// </summary>
        /// <value>
        /// The variable binds.
        /// </value>
        public readonly ReadOnlyCollection<VarBind> VarBinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpV2cPDU"/> struct without decoupling refrence for varbinds.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="varBinds">The variable binds.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="errorStatus">The error status.</param>
        /// <param name="errorIndex">Index of the error.</param>
        internal SnmpV2cPDU(PduType pduType, VarBind[] varBinds, int requestId, SnmpErrorStatus errorStatus, int errorIndex)
        {
            PduType = pduType;
            VarBinds = new ReadOnlyCollection<VarBind>(varBinds);
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpV2cPDU"/> struct with decoupling the reference of varBinds
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="errorStatus">The error status.</param>
        /// <param name="errorIndex">Index of the error.</param>
        /// <param name="varBinds">The variable binds.</param>
        public SnmpV2cPDU(PduType pduType, int requestId, SnmpErrorStatus errorStatus, int errorIndex, VarBind[] varBinds)
        {
            PduType = pduType;
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
            VarBinds = new ReadOnlyCollection<VarBind>((VarBind[])varBinds.Clone());
        }
    }
}
