namespace Tests.Tx.Network
{
    using System;

    using global::Tx.Network;
    using global::Tx.Network.Snmp;

    internal static class Extensions
    {
        internal static SnmpDatagram ToSnmpDatagram(this byte[] source)
        {
            return source.AsByteArraySegment().ToSnmpDatagram(DateTimeOffset.MinValue, "");
        }
    }
}
