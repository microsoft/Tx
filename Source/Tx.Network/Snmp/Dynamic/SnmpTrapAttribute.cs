
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute class that marks a class which holds SNMP trap information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SnmpTrapAttribute : Attribute
    {
        public readonly ObjectIdentifier SnmpTrapOid;

        public SnmpTrapAttribute(string objectIdentifier)
        {
            if (string.IsNullOrWhiteSpace(objectIdentifier))
            {
                throw new ArgumentNullException("objectIdentifier");
            }

            this.SnmpTrapOid = new ObjectIdentifier(objectIdentifier);
        }
    }
}
