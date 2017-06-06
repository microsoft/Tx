namespace Tx.Network.Syslogs
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using Tx.Network;

    public static class SyslogParser
    {
        public static readonly Regex DefaultParser = new Regex(
            @"\<(?<PRIVAL>\d+?)\>\s*(?<MESSAGE>.+)",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        public static Syslog Parse(IUdpDatagram receivedPacket)
        {
            return Parse(receivedPacket.Data.AsByteArraySegment(), receivedPacket.ReceivedTime, receivedPacket.PacketHeader.SourceIpAddress.ToString());
        }

        public static Syslog Parse(ArraySegment<byte> segment, DateTimeOffset receivedTime, string sourceIpAddress)
        {
            if (segment.Array == null)
            {
                throw new ArgumentNullException("segment");
            }

            string logMessage = Encoding.ASCII.GetString(segment.Array, segment.Offset, segment.Count);

            if (string.IsNullOrWhiteSpace(logMessage))
            {
                throw new ArgumentException("Incoming UDP datagram contained no Syslog data.");
            }
            
            var defMatch = DefaultParser.Match(logMessage);
            
            if (!defMatch.Success)
            {
                throw new ArgumentException("Cannot parse the incoming UDP datagram.");
            }

            if (defMatch.Groups.Count < 1)
            {
                throw new ArgumentException("Only no parsable fields in the incoming Syslog");
            }

            var privalMatch = defMatch.Groups["PRIVAL"].Value.Trim();

            if (string.IsNullOrWhiteSpace(privalMatch))
            {
                throw new ArgumentException(
                    "Datagram does not contain the correct string indicating the PRIVAL of the Syslog");
            }

            var prival = int.Parse(privalMatch);
            var severity = (Severity)(prival & 0x7);
            var facility = (Facility)(prival >> 3);
            var message = defMatch.Groups["MESSAGE"].Value.Trim();
            
            return new Syslog(receivedTime, sourceIpAddress, severity, facility, message);
        }
    }
}