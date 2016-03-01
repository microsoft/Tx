
namespace Tx.Network.Snmp
{
    internal static class UdpDatagramExtensions
    {
        public static SnmpDatagram TryParseSnmpDatagram(this UdpDatagram udpDatagram)
        {
            try
            {
                var transportObject = udpDatagram.TransportObject;
                var datagram = transportObject as SnmpDatagram;
                if (datagram == null)
                {
                    datagram = udpDatagram.UdpData.ToSnmpDatagram();
                    udpDatagram.TransportObject = datagram;
                }

                return datagram;
            }
            catch
            {
                return default(SnmpDatagram);
            }
        }
    }
}
