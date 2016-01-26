namespace Tx.Network.Snmp
{
    /// <summary>
    /// This implements Asn1Class types
    /// http://en.wikipedia.org/wiki/X.690#BER_encoding
    /// </summary>
    public enum ConstructType
    {
        /// <summary>
        /// The value is a primitive. Like Integers, OIDs, Boolean, Null.
        /// </summary>
        Primitive = 0,

        /// <summary>
        /// The value is constructed and has special processing or sub-encoding.s
        /// </summary>
        Constructed = 1
    }
}
