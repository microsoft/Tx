
namespace Tx.Network
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Creates an object that will hold a regex built from a dictionary of matching strings and 
    /// can be compiled once at runtime to process incoming Packets.IP or Packets.UdpDatagram objects into Syslog objects. 
    /// </summary>
    public class LogFactory
    {
        #region Public members
        public Dictionary<string, string> FormatElements { get; private set; }
        public Dictionary<string, string> CommonFormatElements { get; private set; }
        public Dictionary<string, string> DefaultFormatElements { get; private set; }
        public Regex FormatMatchRegex { get; private set; }
        #endregion

        #region private members
        static string regexPrival = @"\<(?<PRIVAL>\d+?)\>\s*";
        static string regexCalMonth = @"(?<MONTH>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
        static string regexLegacyDT = @"(?<LEGACYDATETIME>" + regexCalMonth + @"\s(\s\d|\d\d)\s\d{2}:\d{2}:\d{2}\.*\d{0,3}\s*[a-zA-Z]*)";
        static string regexNewDT = @"(?<NEWDATETIME>\ST\SZ*)";
        static string regexLegacyOrNewDT = @"(" + regexLegacyDT + @"|" + regexNewDT + @")";
        static string regexHostname = @"(?< HOSTNAME >[a - zA - Z0 - 9\.\-] +)";
        static string regexIp = @"(?<LOGIP>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
        static string regexHostnameIp = @"(" + regexHostname + @"|" + regexIp + @")";
        #endregion

        #region Constructor
        /// <summary>
        /// Creates new Syslogs using provided or default and common regex parsers.
        /// </summary>
        /// <param name="FormatDictionary">Dictionary of strings to use in parsing logs. Accepts null.</param>
        /// <param name="IncludeDefaults">Add default strings for parsing logs from DefaultFormatElements. If true, keys in FormatParsingDictionary will override default values.</param>
        /// <param name="IncludeCommon">Add common element strings for parsing from CommonFormatElements</param>
        public LogFactory(Dictionary<string, string> FormatDictionary, bool IncludeDefaults, bool IncludeCommon)
        {
            if (!(IncludeDefaults || IncludeDefaults)) return;
            MakeCommonFormatDictionary();
            MakeDefaultFormatDictionary();
            if (IncludeCommon) FormatElements.Concat(CommonFormatElements);
            if (IncludeDefaults) FormatElements.Concat(DefaultFormatElements);
            if (FormatDictionary != null)
            {
                foreach (var kv in FormatDictionary)
                {
                    if (FormatElements.ContainsKey(kv.Key))
                    {
                        FormatElements.Remove(kv.Key);
                    }
                }
            }
            FormatElements.Concat(FormatDictionary);
            FormatMatchRegex = new Regex(formatMatchString(), RegexOptions.Compiled);
        }
        public LogFactory(bool IncludeDefaults, bool IncludeCommon) : this(null, IncludeDefaults, IncludeCommon) { }
        #endregion

        #region Public Methods
        public Syslog GenLog(IpPacket packet)
        {
            return new Syslog(packet, FormatMatchRegex);
        }
        public Syslog GenLog(UdpDatagram packet)
        {
            return new Syslog(packet, FormatMatchRegex);
        }
        private void MakeDefaultFormatDictionary()
        {
            DefaultFormatElements = new Dictionary<string, string>();

            //XR code from Cisco
            string regexIosXr =
                @"(?<STYLE_IOSXR>(?<SEQUENCE>\d+): (?<HOSTNAME>\S+) (?<NODEINTERNAL>\S+):" +
                regexLegacyDT +
                @"\s:\s" +
                @"(?<PROCESS>\S+)\[(?<PROCID>\d+)\]:\s*" +
                @"(?<CATEGORY>%\S+) : " +
                @"(?<MESSAGE>.*))";
            DefaultFormatElements.Add("IOSXR", regexIosXr);

            //typical BSD (Juniper, F5, Arista use this)
            string regexBSD =
                @"(?<STYLE_BSD>(?<SLVERSION>\d{1,2}){0,1}\s*" +
                regexLegacyOrNewDT +
                @"\s" +
                regexHostnameIp +
                @"\s" +
                @"((?<PROCESS>\S+)\[(?<PROCID>\d+)\]:|(?<PROCESS>\S+):|:)*?\s*" +
                @"(?<CATEGORY>%\S+):" +
                @"(?<MESSAGE>.*))";
            DefaultFormatElements.Add("BSD", regexBSD);

            //atypical BSD format (LB4m sometimes, sometimes NXOS)
            string regexBSDNoCategory =
                @"(?<STYLE_NoCategoryBSD>(?<SLVERSION>\d{1,2}){0,1}\s*" +
                regexLegacyOrNewDT +
                @"\s" +
                regexHostnameIp +
                @"\s" +
                @"((?<PROCESS>\S+)\[(?<PROCID>\d+)\]:|(?<PROCESS>\S+):|:|)*?\s*" +
                @"(?<MESSAGE>.*))";
            DefaultFormatElements.Add("NoCategoryBSD", regexBSDNoCategory);

            //atypypical bsd format with no time (LB4m sometimes, sometimes F5)
            string regexBSDNoTime =
                @"(?<STYLE_NoTimeBSD>(?<SLVERSION>\d{1,2}){0,1}\s*" +
                regexHostnameIp +
                @"\s" +
                @"(?<PROCESS>\S+)\[(?<PROCID>\d+)\]:\s*(?<CATEGORY>%\S+):" +
                @"(?<MESSAGE>.*))";
            DefaultFormatElements.Add("NoTimeBSD", regexBSDNoTime);

            //normal Cisco
            string regexIos = @"(?<STYLE_IOS>(?<SEQUENCE>\d+): " +
                regexLegacyDT +
                regexHostnameIp +
                @":\s*" +
                @"(?<CATEGORY>%\S+):" +
                @"(?<MESSAGE>.*))";
            DefaultFormatElements.Add("IOS", regexIos);

            //NXOS internal process ids
            string regexNxos = @"(?<STYLE_NXOS>(?<CATEGORY>%\S+): (?<EventName>\S+):(?<PROCID>\d{1,3}):(?<Message>.*))";
            DefaultFormatElements.Add("NXOS", regexNxos);

            //matches anything that fallsthrough
            string regexAny = @"(?<STYLE_ANY>(?<MESSAGE>.*))";
            DefaultFormatElements.Add("ANY", regexAny);


        }
        #endregion

        #region Private Methods
        private void MakeCommonFormatDictionary()
        {
            CommonFormatElements = new Dictionary<string, string>();

            CommonFormatElements.Add("PRIVAL", regexPrival);
            CommonFormatElements.Add("MONTH", regexCalMonth);
            CommonFormatElements.Add("LEGACYDATETIME", regexLegacyDT);
            CommonFormatElements.Add("NEWDATETIME", regexNewDT);
            CommonFormatElements.Add("HOSTNAME", regexHostname);
            CommonFormatElements.Add("LOGIP", regexIp);
        }

        private string formatMatchString()
        {
            return string.Concat(@"(", string.Join(@"|", FormatElements.Values), @")");
        }
        #endregion
    }
}
