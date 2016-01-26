namespace Tx.Network.Snmp
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Class to represent PDU for Snmp V2C and V1 data
    /// </summary>
    public struct SnmpPDU
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
        /// Initializes a new instance of the <see cref="SnmpPDU"/> struct without decoupling refrence for varbinds.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="varBinds">The variable binds.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="errorStatus">The error status.</param>
        /// <param name="errorIndex">Index of the error.</param>
        internal SnmpPDU(PduType pduType, VarBind[] varBinds, int requestId, SnmpErrorStatus errorStatus, int errorIndex)
        {
            PduType = pduType;
            VarBinds = new ReadOnlyCollection<VarBind>(varBinds);
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpPDU"/> struct with decoupling the reference of varBinds
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="errorStatus">The error status.</param>
        /// <param name="errorIndex">Index of the error.</param>
        /// <param name="varBinds">The variable binds.</param>
        public SnmpPDU(PduType pduType, int requestId, SnmpErrorStatus errorStatus, int errorIndex, VarBind[] varBinds)
        {
            PduType = pduType;
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
            VarBinds = new ReadOnlyCollection<VarBind>((VarBind[])varBinds.Clone());
        }

        /// <summary>
        /// Searches the first sub oid.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        /// <param name="varBind">The variable bind.</param>
        /// <returns>Boolean value true if subOid is found else false</returns>
        public bool SearchFirstSubOidWith(ObjectIdentifier subOid, out VarBind varBind)
        {
            bool isFound = false;
            varBind = default(VarBind);
            for (int i = 0; i < VarBinds.Count; i++)
            {
                if(VarBinds[i].Oid.IsSubOid(subOid))
                {
                    varBind = VarBinds[i];
                    isFound = true;
                    break;
                }
            }
            
            return isFound;
        }

        /// <summary>
        /// Searches the last sub oid with.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        /// <param name="varBind">The variable bind.</param>
        /// <returns>Boolean value true if subOid is found else false</returns>
        public bool SearchLastSubOidWith(ObjectIdentifier subOid, out VarBind varBind)
        {
            bool isFound = false;
            varBind = default(VarBind);
            for (int i = VarBinds.Count -1; i >= 0; i--)
            {
                if (VarBinds[i].Oid.IsSubOid(subOid))
                {
                    varBind = VarBinds[i];
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        /// <summary>
        /// Gets all oids starting with.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        /// <returns>IEnumerable of VarBind</returns>
        public IEnumerable<VarBind> GetAllOidsStartingWith(ObjectIdentifier subOid)
        {
            for (int i = 0; i < VarBinds.Count; i++)
            {
                if (VarBinds[i].Oid.IsSubOid(subOid))
                {
                    yield return VarBinds[i];
                }
            }
        }
    }
}
