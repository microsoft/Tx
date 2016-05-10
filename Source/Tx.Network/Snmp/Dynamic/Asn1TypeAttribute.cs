
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute used to mark Asn1Type tags property of an MIB table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Asn1TypeAttribute : Attribute
    {
        public readonly Asn1TagInfo tagInfo;

        public Asn1TypeAttribute(Asn1Tag asn1TagInfo)
        {
            this.tagInfo = new Asn1TagInfo(asn1TagInfo);
        }

        public Asn1TypeAttribute(Asn1SnmpTag asn1SnmpTag)
        {
            this.tagInfo = new Asn1TagInfo(asn1SnmpTag);
        }

        public Asn1TypeAttribute(Asn1SnmpTag asn1SnmpTag, ConstructType constructType)
        {
            this.tagInfo = new Asn1TagInfo(asn1SnmpTag, constructType);
        }

        public Asn1TypeAttribute(Asn1Tag asn1Tag, ConstructType constructType, Asn1Class asn1Class)
        {
            this.tagInfo = new Asn1TagInfo(asn1Tag, constructType, asn1Class);
        }
    }
}
