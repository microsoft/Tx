
namespace Tx.Network.Snmp.Dynamic
{
    using System;

    /// <summary>
    /// Attribute used to mark property of an MIB table as index for storage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StorageIndexAttribute : Attribute
    {
    }
}
