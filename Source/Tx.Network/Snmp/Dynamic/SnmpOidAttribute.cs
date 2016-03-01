
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute that marks a property that maps to an SNMP object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SnmpOidAttribute : Attribute
    {
        public ObjectIdentifier Oid { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="SnmpOidAttribute"/> with the given object identifier.
        /// </summary>
        /// <param name="oid">The identifier of the object that is represented by the attributed property.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="oid"/> is null or whitespace.</exception>
        public SnmpOidAttribute(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
            {
                throw new ArgumentNullException("oid");
            }

            this.Oid = new ObjectIdentifier(oid);
        }
    }
}
