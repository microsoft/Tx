
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute that marks a property whose value is all varbinds that came in the original trap.
    /// </summary>
    /// <remarks>
    /// The property must be assignable from <see cref="SnmpDatagram.VarBinds"/>. (ie. a ReadOnlyCollection)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotificationObjectsAttribute : Attribute
    {
    }
}
