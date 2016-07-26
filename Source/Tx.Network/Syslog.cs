
namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;

    #region Enums

    [Obsolete("Deprecated. Use Tx.Network.Syslogs.Severity instead.")]
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

    [Obsolete("Deprecated. Use Tx.Network.Syslogs.Facility instead.")]
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
    [Obsolete("Deprecated. Use Tx.Network.Syslogs.Syslog instead.")]
    public class Syslog : UdpDatagram
    {
        #region Public Members

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

        /// <summary>
        /// Default Regex Options.
        /// </summary>
        public static readonly RegexOptions DefaultOptions = RegexOptions.CultureInvariant
                                                             | RegexOptions.ExplicitCapture;

        /// <summary>
        /// Default parser resulting in output of Severity, Facility, and the Message string following the PRIVAL.
        /// </summary>
        public static readonly Regex DefaultParser = new Regex(@"\<(?<PRIVAL>\d+?)\>\s*(?<MESSAGE>.+)", DefaultOptions);

        #region Constructors

        /// <summary>
        /// Creates a default instance of the Log object.
        /// </summary>
        public Syslog()
            : base()
        {
            Parser = DefaultParser;
            LogSeverity = Severity.Debug;
            LogFacility = Facility.local7;
            NamedCollectedMatches = new Dictionary<string, Group>();
        }

        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="Buffer">A byte array containing data received on a Raw socket.</param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(byte[] Buffer, Regex Parser)
            : this(new UdpDatagram(Buffer), Parser)
        {
        }

        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="ReceivedPacket">An IP object generated on data received on a Raw socket. </param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(IpPacket ReceivedPacket, Regex Parser)
            : this(new UdpDatagram(ReceivedPacket), Parser)
        {
        }

        /// <summary>
        /// Creates an instance of a Log object.
        /// </summary>
        /// <param name="ReceivedPacket">An UdpDatagram object generated on data received on a Raw socket. </param>
        /// <param name="Parser">A regular expression that will be used to parse the internal message of the Log.</param>
        public Syslog(UdpDatagram ReceivedPacket, Regex Parser)
            : base(ReceivedPacket)
        {
            if (Protocol != ProtocolType.Udp)
            {
                throw new NotSupportedException("Datagrams other than UDP not supported to generate Syslogs.");
            }

            string LogMessage = Encoding.ASCII.GetString(UdpData);

            if (string.IsNullOrWhiteSpace(LogMessage))
            {
                throw new ArgumentOutOfRangeException("Incoming UDP datagram contained no Syslog data.");
            }

            Match defMatch = DefaultParser.Match(LogMessage);
            var privalMatch = defMatch.Groups["PRIVAL"].Value.Trim();

            if (!string.IsNullOrWhiteSpace(privalMatch))
            {
                var prival = int.Parse(privalMatch);
                LogSeverity = (Severity)Enum.ToObject(typeof(Severity), prival & 0x7);
                LogFacility = (Facility)Enum.ToObject(typeof(Facility), prival >> 3);
                Message = defMatch.Groups["MESSAGE"].Value.Trim();
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "Datagram does not contain the correct string indicating the PRIVAL of the Syslog");
            }

            if (SetRegex(Parser))
            {
                NamedCollectedMatches = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);

                //support needed for other encoding per IETF RFC.
                Match matchMe = this.Parser.Match(LogMessage);
                if (matchMe.Groups.Count < 1) throw new Exception("Only no parsable fields in the incoming Syslog");

                foreach (var groupName in Parser.GetGroupNames())
                {
                    if (string.IsNullOrEmpty(matchMe.Groups[groupName].Value)) continue;
                    NamedCollectedMatches.Add(groupName, matchMe.Groups[groupName]);
                }
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
        public bool SetRegex(Regex Parser)
        {
            if (Parser == default(Regex) || string.IsNullOrWhiteSpace(Parser.ToString()))
            {
                this.Parser = DefaultParser;
                return false;
            }
            this.Parser = Parser;
            return true;
        }

        #endregion
    }

    namespace Syslogs
    {
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
            Kernel = 0,

            UserLevel,

            MailSystem,

            SystemDaemons,

            Authorization,

            Syslog,

            Printer,

            News,

            Uucp,

            Clock,

            SecurityAuth,

            Ftp,

            Ntp,

            LogAudit,

            LogAlert,

            ClockDaemon,

            Local0,

            Local1,

            Local2,

            Local3,

            Local4,

            Local5,

            Local6,

            Local7
        }

        /// <summary>
        /// An object representing a processed Syslog message.
        /// </summary>
        public class Syslog
        {
            /// <summary>
            /// Received DateTime TimeStamp
            /// </summary>
            public DateTimeOffset ReceivedTime { get; private set; }

            /// <summary>
            /// The IP Address of the station that sent the Syslog to the local receiver.
            /// </summary>
            public string SourceIpAddress { get; private set; }

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
            public IDictionary<string, string> NamedCollectedMatches { get; private set; }

            /// <summary>
            /// Creates a default instance of the Log object.
            /// </summary>
            public Syslog(
                DateTimeOffset receivedTime,
                string sourceIpAddress,
                Severity severity,
                Facility facility,
                string message,
                IDictionary<string, string> namedCollectedMatches)
            {
                ReceivedTime = receivedTime;
                SourceIpAddress = sourceIpAddress;
                LogSeverity = severity;
                LogFacility = facility;
                Message = message;
                NamedCollectedMatches = namedCollectedMatches;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.AppendLine();

                if (NamedCollectedMatches != null)
                {
                    foreach (var c in NamedCollectedMatches)
                    {
                        sb.AppendFormat(c.Key).Append(", ");
                        sb.AppendLine(c.Value);
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            }
        }

        public class SyslogParser
        {
            public static readonly Regex DefaultParser = new Regex(
                @"\<(?<PRIVAL>\d+?)\>\s*(?<MESSAGE>.+)",
                RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture| RegexOptions.Compiled);

            private readonly Regex parser;

            private readonly string[] groupNames;

            public SyslogParser()
                : this(DefaultParser)
            {
            }

            public SyslogParser(Regex parser)
            {
                if (parser == null)
                {
                    throw new ArgumentNullException("parser");
                }

                this.parser = parser;
                this.groupNames = parser.GetGroupNames();
            }

            public Syslog Parse(UdpDatagram receivedPacket)
            {
                if (receivedPacket.Protocol != ProtocolType.Udp)
                {
                    throw new NotSupportedException("Datagrams other than UDP not supported to generate Syslogs.");
                }

                string logMessage = Encoding.ASCII.GetString(receivedPacket.UdpData);

                if (string.IsNullOrWhiteSpace(logMessage))
                {
                    throw new ArgumentException("Incoming UDP datagram contained no Syslog data.");
                }

                var defMatch = this.parser.Match(logMessage);

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
                var severity = (Severity)Enum.ToObject(typeof(Severity), prival & 0x7);
                var facility = (Facility)Enum.ToObject(typeof(Facility), prival >> 3);
                var message = defMatch.Groups["MESSAGE"].Value.Trim();

                var matches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var groupName in this.groupNames)
                {
                    var group = defMatch.Groups[groupName];

                    if (group.Success && !string.IsNullOrEmpty(group.Value))
                    {
                        matches[groupName] = group.Value;
                    }
                }

                return new Syslog(receivedPacket.ReceivedTime, receivedPacket.SourceIpAddress.ToString(), severity, facility, message, matches);
            }
        }
    }
}