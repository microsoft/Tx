using System;
using System.Collections.Generic;

namespace Tx.Network.Snmp
{
    public static class SnmpCapture
    {
        /// <summary>
        /// Reads capture from file that contains only SNMP
        /// To get such file, use WireShark to export only packets that are SNMP
        /// </summary>
        /// <param name="file">The file in pcap-next-generation (.pacapng) format</param>
        /// <returns></returns>
        public static IEnumerable<PDU> ReadPcapNg(string file)
        {
            var records = PcapNg.ReadForward(file);

            foreach (var r in records)
            {
                var p = r as EnhancedPacketBlock;
                if (p == null)
                    continue;

                int snmpLen = p.PacketData.Length - 42;

                byte[] datagram = new byte[snmpLen];
                Array.Copy(p.PacketData, 42, datagram, 0, snmpLen);

                yield return new PDU(datagram);
            }
        }
    }
}
