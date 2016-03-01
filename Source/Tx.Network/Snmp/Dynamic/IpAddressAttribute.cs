
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute used to mark the property of an SNMP trap class that holds an IpAddress.
    /// </summary>
    /// <remarks>
    /// The property must be either an <see cref="System.Net.IPAddress"/> or <see cref="String"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IpAddressAttribute : Attribute
    {
    }
}
