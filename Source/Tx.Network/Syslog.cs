
namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;

    #region Enums
    public enum Severity : byte
    {
        Emergency = 0,
        Alert,
        Critical,
        Error,
        Warning,
        Notice,
        Informational,
        Debug
    }
    public enum Facility : byte
    {
        kernel = 0,
        userlevel,
        mailsystem,
        systemdaemons,
        authorization,
        syslog,
        printer,
        news,
        uucp,
        clock,
        securityauth,
        ftp,
        ntp,
        logaudit,
        logalert,
        clockdaemon,
        local0,
        local1,
        local2,
        local3,
        local4,
        local5,
        local6,
        local7
    }
    #endregion
    /// <summary>
    /// An object representing a processed UdpDatagram Syslog message.
    /// </summary>
    /// <seealso>Syslog.LogFactory class.</seealso>
    public class Syslog : UdpDatagram
    {
        #region Public Members
        /// <summary>
        /// The time the datagram was processed by the Syslog parser.
        /// </summary>
        public DateTime ReceivedTime { get; set; }

        /// <summary>
        /// The message contained in the datagram following the PRIVAL
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Severity of the Syslog provided from the PRIVAL field.
        /// </summary>
        public Severity LogSeverity { get; private set; }

        /// <summary>
        /// Facility of the Syslog provided from the PRIVAL field.
        /// </summary>
        public Facility LogFacility { get; private set; }
        
        /// <summary>
        /// Collection of regular expression matches.
        /// </summary>
        public Dictionary<string, Group> NamedCollectedMatches { get; private set; }
        
        /// <summary>
        /// Regular expression to use to parse the Syslog message.
        /// </summary>
        public Regex Parser { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a default instance of the Log object.
        /// </summary>
        public Syslog() : base()
        {
            LogSeverity = Severity.Debug;
            LogFacility = Facility.local7;
            NamedCollectedMatches =  new Dictionary<string, Group>();   
          
        }
        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="Buffer">A byte array containing data received on a Raw socket.</param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(byte[] Buffer, Regex Parser) : this(new UdpDatagram(Buffer), Parser) { }

        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="ReceivedPacket">An IP object generated on data received on a Raw socket. </param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(IpPacket ReceivedPacket, Regex Parser) : this(new UdpDatagram(ReceivedPacket), Parser) { }

        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="ReceivedPacket">An UdpDatagram object generated on data received on a Raw socket. </param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(UdpDatagram ReceivedPacket, Regex Parser) : base(ReceivedPacket)
        {
            NamedCollectedMatches = new Dictionary<string, Group>();
            if (Protocol != ProtocolType.Udp) return;

            SetRegex(Parser);
            string LogMessage = Encoding.ASCII.GetString(UdpData); //should add support for other encoding per IETF RFC.
            Match matchMe = Parser.Match(LogMessage);
            if (matchMe.Groups.Count < 1) throw new Exception("Only no parsable fields in the incoming Syslog");
            
            //Special priority-value handler to decode into Severity and Facility.
            var pval = Regex.Match(LogMessage, @"\<(?<PRIVAL>\d+?)\>\s*(?<MESSAGE>.+)");
            if (!String.IsNullOrEmpty(pval.Groups["PRIVAL"].Value.Trim()))
            {
                var prival = int.Parse(pval.Groups["PRIVAL"].Value.Trim());
                LogSeverity = (Severity)Enum.ToObject(typeof(Severity), prival & 0x7);
                LogFacility = (Facility)Enum.ToObject(typeof(Facility), prival >> 3);
                this.Message = pval.Groups["MESSAGE"].Value.Trim();
            }
            
            foreach (var groupName in Parser.GetGroupNames())
            {
                if (string.IsNullOrEmpty(matchMe.Groups[groupName].Value)) continue;
                NamedCollectedMatches.Add(groupName, matchMe.Groups[groupName]);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine();
            foreach (var c in NamedCollectedMatches)
            {
                sb.AppendFormat("{0}, ", c.Key);
                sb.AppendLine(c.Value.Value);
                sb.AppendLine();
            }
            return sb.ToString();

        }

        #endregion

        #region Methods
        /// <summary>
        /// Sets the regular expression used to parse the Syslog message. No update is made if the regex 
        /// is default or has an empty regex string.
        /// </summary>
        /// <param name="Parser">The regular expression that provides parsing of the Syslog message.</param>
        public void SetRegex(Regex Parser)
        {
            if (String.IsNullOrEmpty(Parser.ToString()) || Parser == default(Regex)) return;
            this.Parser = Parser;
        }
        #endregion
    }
}
