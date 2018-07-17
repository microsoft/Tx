namespace Tx.Network.Syslogs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
        public IReadOnlyDictionary<string, string> NamedCollectedMatches { get; private set; }

        /// <summary>
        /// Creates a default instance of the Log object.
        /// </summary>
        public Syslog(
            DateTimeOffset receivedTime,
            string sourceIpAddress,
            Severity severity,
            Facility facility,
            string message,
            IReadOnlyDictionary<string, string> namedCollectedMatches)
        {
            this.ReceivedTime = receivedTime;
            this.SourceIpAddress = sourceIpAddress;
            this.LogSeverity = severity;
            this.LogFacility = facility;
            this.Message = message;
            this.NamedCollectedMatches = namedCollectedMatches;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            if (this.NamedCollectedMatches != null)
            {
                foreach (var c in this.NamedCollectedMatches)
                {
                    sb.AppendFormat(c.Key).Append(", ");
                    sb.AppendLine(c.Value);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}