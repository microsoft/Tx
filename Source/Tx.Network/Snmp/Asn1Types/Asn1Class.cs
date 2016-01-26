namespace Tx.Network.Snmp
{
    /// <summary>
    /// This implements Asn1Class types
    /// http://en.wikipedia.org/wiki/X.690#BER_encoding
    /// </summary>
    public enum Asn1Class
    {
        /// <summary>
        /// The universal
        /// </summary>
        Universal = 0,

        /// <summary>
        /// The application
        /// </summary>
        Application = 1,

        /// <summary>
        /// The context specific
        /// </summary>
        ContextSpecific = 2,

        /// <summary>
        /// The private
        /// </summary>
        Private = 3
    }
}
