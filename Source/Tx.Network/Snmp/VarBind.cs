namespace Tx.Network.Snmp
{
    /// <summary>
    /// Immutable value type to hold Asn1Data
    /// </summary>
    public struct VarBind
    {
        /// <summary>
        /// The ObjectIdentifier for Varbind
        /// </summary>
        public readonly ObjectIdentifier Oid;

        /// <summary>
        /// The value for Varbind
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// The Asn1TagInfo to provide Asn.1\Snmp tag information
        /// </summary>
        public readonly Asn1TagInfo Asn1TypeInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="VarBind" /> struct.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <param name="value">The value.</param>
        /// <param name="asn1TypeInfo">The asn1 type information.</param>
        public VarBind(ObjectIdentifier oid, object value, Asn1TagInfo asn1TypeInfo)
        {
            Oid = oid;
            Value = value;
            Asn1TypeInfo = asn1TypeInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VarBind"/> struct with value null and Asn1.Null type.
        /// </summary>
        /// <param name="oid">The oid.</param>
        public VarBind(ObjectIdentifier oid)
        {
            Oid = oid;
            Value = null;
            Asn1TypeInfo = new Asn1TagInfo(Asn1Tag.Null);
        }
    }
}
