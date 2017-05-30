using System;
using Tx.Network.Snmp;

namespace Tx.Network.Tests
{
    internal static class Extensions
    {
        internal static SnmpDatagram ToSnmpDatagram(this byte[] source)
        {
            return source.AsByteArraySegment().ToSnmpDatagram(DateTimeOffset.MinValue, string.Empty);
        }
    }
}
