namespace Tx.Network.Snmp
{
    /// <summary>
    /// This implements Asn1Class types
    /// http://en.wikipedia.org/wiki/X.690#BER_encoding
    /// </summary>
    public enum Asn1Tag
    {
        /// <summary>
        /// The end of content
        /// </summary>
        EndOfContent = 0,

        /// <summary>
        /// The boolean
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// The integer
        /// </summary>
        Integer = 2,

        /// <summary>
        /// The bit string
        /// </summary>
        BitString = 3,

        /// <summary>
        /// The octet string
        /// </summary>
        OctetString = 4,

        /// <summary>
        /// The null
        /// </summary>
        Null = 5,

        /// <summary>
        /// The object identifier
        /// </summary>
        ObjectIdentifier = 6,

        /// <summary>
        /// The sequence
        /// </summary>
        Sequence = 16,

        /// <summary>
        /// The set of
        /// </summary>
        SetOf = 17,

        /// <summary>
        /// The numeric string
        /// </summary>
        NumericString = 18,

        /// <summary>
        /// The printable string
        /// </summary>
        PrintableString = 19,

        /// <summary>
        /// The T61 string
        /// </summary>
        T61String = 20,

        /// <summary>
        /// The videotex string
        /// </summary>
        VideotexString = 21,

        /// <summary>
        /// The i a5 string
        /// </summary>
        IA5String = 22,

        /// <summary>
        /// The UTC time
        /// </summary>
        UTCTime = 23,

        /// <summary>
        /// The generalized time
        /// </summary>
        GeneralizedTime = 24,

        /// <summary>
        /// The graphic string
        /// </summary>
        GraphicString = 25,

        /// <summary>
        /// The visible string
        /// </summary>
        VisibleString = 26,

        /// <summary>
        /// The general string
        /// </summary>
        GeneralString = 27,

        /// <summary>
        /// The universal string
        /// </summary>
        UniversalString = 28,

        /// <summary>
        /// The character string
        /// </summary>
        CharacterString = 29,

        /// <summary>
        /// The BMP string
        /// </summary>
        BMPString = 30,

        /// <summary>
        /// The not asn1 data
        /// This is custom Tag added just to identify if data is Snmp or Asn1
        /// </summary>
        NotAsn1Data = 99
    }
}
