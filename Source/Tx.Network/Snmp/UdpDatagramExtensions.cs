namespace Tx.Network.Snmp
{
    using Tx.Network;

    internal static class UdpDatagramExtensions
    {
        public static bool TryParseSnmpDatagram(this IUdpDatagram udpDatagram, out SnmpDatagram snmpDatagram)
        {
            try
            {
                var segment = udpDatagram.Data.AsByteArraySegment();
                snmpDatagram = segment.ToSnmpDatagram(udpDatagram.ReceivedTime, udpDatagram.PacketHeader.SourceIpAddress.ToString());
                return true;
            }
            catch
            {
                snmpDatagram = default(SnmpDatagram);
                return false;
            }
        }
    }
}
