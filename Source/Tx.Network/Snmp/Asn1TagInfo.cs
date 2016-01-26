
namespace Tx.Network.Snmp
{
    /// <summary>
    /// Immutable structure for Asn1TagInfo
    /// </summary>
    public struct Asn1TagInfo
    {
        /// <summary>
        /// The asn1 class type
        /// </summary>
        public readonly Asn1Class Asn1ClassType;

        /// <summary>
        /// The asn1 construct type
        /// </summary>
        public readonly ConstructType Asn1ConstructType;

        /// <summary>
        /// The asn1 tag type
        /// </summary>
        public readonly Asn1Tag Asn1TagType;

        /// <summary>
        /// The asn1 SNMP tag type
        /// </summary>
        public readonly Asn1SnmpTag Asn1SnmpTagType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Class">The asn1 class.</param>
        /// <param name="constructType">Type of the construct.</param>
        /// <param name="tagType">Type of the tag.</param>
        public Asn1TagInfo(int asn1Class, int constructType, int tagType)
        {
            Asn1ClassType = (Asn1Class)asn1Class;
            Asn1ConstructType = (ConstructType)constructType;
            if (Asn1ClassType == Asn1Class.Application)
            {
                Asn1SnmpTagType = (Asn1SnmpTag)tagType;
                Asn1TagType = Asn1Tag.NotAsn1Data;
            }
            else
            {
                Asn1TagType = (Asn1Tag)tagType;
                Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
            }
        }
    }
}
