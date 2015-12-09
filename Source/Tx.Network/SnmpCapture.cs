using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Tx.Network.Snmp
{
    public static class SnmpCapture
    {
        /// <summary>
        /// Reads the SNMP packets send over UDP, over IPv4, over Ethernet
        /// from capture file, ignoring everything else.
        /// 
        /// All SNMP v2c packets are returned. In case SNMP v1 traps are ignored (NYI)
        /// </summary>
        /// <param name="file">The file in pcap-next-generation (.pacapng) format</param>
        /// <returns></returns>
        public static IEnumerable<PDU> ReadPcapNg(string file)
        {
            return PcapNg.ReadForward(file).ParseSnmp();
        }

        public static IEnumerable<IpPacket> ParseIP(this IEnumerable<Block> capture)
        {
            return capture.Where(b => b.Type == BlockType.EnhancedPacketBlock)
                        .Cast<EnhancedPacketBlock>()
                        .Where(p => p.InterfaceDescription.LinkType == LinkType.ETHERNET)
                        .Where(p => p.PacketData.ReadNetOrderUShort(0xc) == 0x800)
                        .Select(p =>
                        {
                            int ipLen = p.PacketData.Length - 14;
                            byte[] ipData = new byte[ipLen];
                            Array.Copy(p.PacketData, 14, ipData, 0, ipLen);
                            return new IpPacket(ipData);
                        });
        }

        public static IEnumerable<UdpDatagram> ParseUdp(this IEnumerable<Block> capture)
        {
            return capture.ParseIP()
                .Where(p => p.Protocol == ProtocolType.Udp)
                .Select(p => new UdpDatagram(p));
        }

        public static IEnumerable<PDU> ParseSnmp(this IEnumerable<Block> capture)
        {
            int unreadablePackets = 0; // for debugging

            foreach (UdpDatagram p in capture.ParseUdp())
            {
                PDU pdu = null;
                try
                {
                    pdu = new PDU(p);
                }
                catch (Exception ex)
                {
                    unreadablePackets++;
                    continue;
                }

                yield return pdu;
            }
        }
    }
}
