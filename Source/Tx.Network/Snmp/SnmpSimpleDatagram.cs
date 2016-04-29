namespace Tx.Network.Snmp
{
    using System.Collections.Generic;

    /// <summary>
    /// Simple class to represent SnmpDatagram
    /// </summary>
    public class SnmpSimpleDatagram
    {
        /// <summary>
        /// The SnmpDatagram
        /// </summary>
        private SnmpDatagram snmpDatagram;

        /// <summary>
        /// Gets the snmp version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public readonly SnmpVersion Version;

        /// <summary>
        /// Gets the community string.
        /// </summary>
        /// <value>
        /// The community string.
        /// </value>
        public readonly string Community;
        /// <summary>
        /// Gets the type of the pdu.
        /// </summary>
        /// <value>
        /// The type of the pdu.
        /// </value>
        public readonly PduType PDUType;

        /// <summary>
        /// Gets the request identifier number.
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
        /// Gets the IList of variable binds.
        /// </summary>
        /// <value>
        /// The variable binds.
        /// </value>
        public readonly IList<KeyValuePair<string,object>> VarBinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpSimpleDatagram" /> class.
        /// </summary>
        /// <param name="snmpDatagramInput">The SNMP datagram input.</param>
        public SnmpSimpleDatagram(SnmpDatagram snmpDatagramInput)
        {
            snmpDatagram = snmpDatagramInput;
            Version = snmpDatagram.Header.Version;
            Community = snmpDatagram.Header.Community;
            PDUType = snmpDatagram.PDU.PduType;
            RequestId = snmpDatagram.PDU.RequestId;
            ErrorStatus = snmpDatagram.PDU.ErrorStatus;
            ErrorIndex = snmpDatagram.PDU.ErrorIndex;
            VarBinds = new List<KeyValuePair<string, object>>();
            for(int i=0 ; i< snmpDatagram.PDU.VarBinds.Count; i++)
            {
                var varBind = snmpDatagram.PDU.VarBinds[i];
                VarBinds.Add(new KeyValuePair<string, object>(varBind.Oid.ToString(), varBind.Value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpSimpleDatagram"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public SnmpSimpleDatagram(byte[] bytes)
            : this(bytes.ToSnmpDatagram())
        {

        }

        /// <summary>
        /// Encodes to asn1 byte array.
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] EncodeToAsn1ByteArray()
        {
            return snmpDatagram.ToSnmpEncodedByteArray();
        }

        /// <summary>
        /// Searches the List of VarBinds for first occurrence of input OID.
        /// </summary>
        /// <param name="oid">The objectIdetifier as string.</param>
        /// <param name="varbind">The varbind.</param>
        /// <returns>boolean true if oid is found in VarBinds else false</returns>
        public bool SearchFirstSubOidWith(string oid, out KeyValuePair<string, object> varbind)
        {
            varbind = default(KeyValuePair<string, object>);
            VarBind getVarbind;
            if(snmpDatagram.PDU.SearchFirstSubOidWith(new ObjectIdentifier(oid), out getVarbind))
            {
                varbind = new KeyValuePair<string, object>(getVarbind.Oid.ToString(), getVarbind.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Searches the List of VarBinds for last occurrence of input OID.
        /// </summary>
        /// <param name="oid">The objectIdetifier as string.</param>
        /// <param name="varbind">The varbind.</param>
        /// <returns>boolean true if oid is found in VarBinds else false</returns>
        public bool SearchLastSubOidWith(string oid, out KeyValuePair<string, object> varbind)
        {
            varbind = default(KeyValuePair<string, object>);
            VarBind getVarbind;
            if (snmpDatagram.PDU.SearchLastSubOidWith(new ObjectIdentifier(oid), out getVarbind))
            {
                varbind = new KeyValuePair<string, object>(getVarbind.Oid.ToString(), getVarbind.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all oids which starts with input oid.
        /// </summary>
        /// <param name="oid">The objectIdetifier as string.</param>
        /// <returns>
        /// boolean true if oid is found in VarBinds else false
        /// </returns>
        public IList<KeyValuePair<string, object>> GetAllOidsStartingWith(string oid)
        {
            var varBinds = new List<KeyValuePair<string, object>>();
            foreach (var varBind in snmpDatagram.PDU.GetAllOidsStartingWith(new ObjectIdentifier(oid)))
            {
                varBinds.Add(new KeyValuePair<string, object>(varBind.Oid.ToString(), varBind.Value));
            }

            return varBinds;
        }
    }
}
