
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
            this.Asn1ClassType = (Asn1Class)asn1Class;
            this.Asn1ConstructType = (ConstructType)constructType;
            if (this.Asn1ClassType == Asn1Class.Application)
            {
                this.Asn1SnmpTagType = (Asn1SnmpTag)tagType;
                this.Asn1TagType = Asn1Tag.NotAsn1Data;
            }
            else
            {
                this.Asn1TagType = (Asn1Tag)tagType;
                this.Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
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
            this.Asn1ClassType = Asn1Class.Application;
            this.Asn1ConstructType = constructType;
            this.Asn1SnmpTagType = asn1SnmpTag;
            this.Asn1TagType = Asn1Tag.NotAsn1Data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Tag">The asn1 tag.</param>
        /// <param name="constructType">Type of the construct.</param>
        public Asn1TagInfo(Asn1Tag asn1Tag, ConstructType constructType)
        {
            this.Asn1ClassType = Asn1Class.Universal;
            this.Asn1ConstructType = constructType;
            this.Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
            this.Asn1TagType = asn1Tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1TagInfo"/> struct.
        /// </summary>
        /// <param name="asn1Tag">The asn1 tag.</param>
        /// <param name="constructType">Type of the construct.</param>
        /// <param name="asn1Class">The asn1 class.</param>
        public Asn1TagInfo(Asn1Tag asn1Tag, ConstructType constructType, Asn1Class asn1Class)
        {
            this.Asn1ClassType = asn1Class;
            this.Asn1ConstructType = constructType;
            this.Asn1SnmpTagType = Asn1SnmpTag.NotSnmpData;
            this.Asn1TagType = asn1Tag;
        }
    }
}
