
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

        public Asn1TypeAttribute(Asn1TagInfo asn1TagInfo)
        {
            this.tagInfo = asn1TagInfo;
        }
    }
}
