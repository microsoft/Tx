
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
        internal Asn1TagInfo(int asn1Class, int constructType, int tagType)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1SnmpTag">The asn1 SNMP tag.</param>
        public Asn1TagInfo(Asn1SnmpTag asn1SnmpTag) :this(asn1SnmpTag, ConstructType.Primitive) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Tag">The asn1 SNMP tag.</param>
        public Asn1TagInfo(Asn1Tag asn1Tag) : this(asn1Tag, ConstructType.Primitive) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1SnmpTag">The asn1 SNMP tag.</param>
        /// <param name="constructType">Type of the construct.</param>
        public Asn1TagInfo(Asn1SnmpTag asn1SnmpTag, ConstructType constructType)
        {
            Asn1ClassType = Asn1Class.Application;
            Asn1ConstructType = constructType;
            Asn1SnmpTagType = asn1SnmpTag;
            Asn1TagType = Asn1Tag.NotAsn1Data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Tag">The asn1 tag.</param>
        /// <param name="constructType">Type of the construct.</param>
        public Asn1TagInfo(Asn1Tag asn1Tag, ConstructType constructType)
        {
            Asn1ClassType = Asn1Class.Universal;
            Asn1ConstructType = constructType;
            Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
            Asn1TagType = asn1Tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Tag">The asn1 tag.</param>
        /// <param name="constructType">Type of the construct.</param>
        /// <param name="asn1Class">The asn1 class.</param>
        public Asn1TagInfo(Asn1Tag asn1Tag, ConstructType constructType, Asn1Class asn1Class)
        {
            Asn1ClassType = asn1Class;
            Asn1ConstructType = constructType;
            Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
            Asn1TagType = asn1Tag;
        }
    }
}
