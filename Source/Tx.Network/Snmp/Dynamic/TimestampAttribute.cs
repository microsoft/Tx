
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute that marks a property whose value is the received timestamp of the trap.
    /// </summary>
    /// <remarks>
    /// The property must be assignable from <see cref="IpPacket.ReceivedTime"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class TimestampAttribute : Attribute
    {
    }
}
