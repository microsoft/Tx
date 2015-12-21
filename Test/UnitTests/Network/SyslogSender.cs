
namespace Tx.Network.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Simulates sending Syslogs over the network.
    /// </summary>
    public class TxSyslogSender
    {
        //public int Counter;
        public CancellationToken Cancel { get; set; }
       
        public List<SimpleTxSyslog> SentList { get; private set; }
        public TxSyslogSender()
        {
            Cancel = Task.Factory.CancellationToken;
            SentList = new List<SimpleTxSyslog>();
            
        }

        public Task<int> StartSendAsync(string SourceIp, string TargetIp, int UdpPort, TimeSpan Delay, TimeSpan Duration, List<string> Source)
        {
            return Task.Factory.StartNew<int>(() => { return Send(SourceIp, TargetIp, UdpPort, Delay, Duration, Source); }, Cancel);
        }

        public int Send(string SourceIp, string TargetIp, int UdpPort, TimeSpan Delay, TimeSpan Duration, List<string> Source)
        {
            var start = DateTime.UtcNow;
            var localCounter = 0;
            using (var UC = new UdpClient(new IPEndPoint(IPAddress.Parse(SourceIp), 0)))
            {
                while (DateTime.UtcNow < start.Add(Duration))
                {
                    var txtMsg = Source[localCounter % Source.Count];
                    var msg = Encoding.ASCII.GetBytes(txtMsg);
                    var defMatch = Syslog.DefaultParser.Match(txtMsg);
                    var privalMatch = defMatch.Groups["PRIVAL"].Value.Trim();
                    var prival = int.Parse(privalMatch);
                    var sent = new SimpleTxSyslog()
                    {
                        Sev = (Severity)Enum.ToObject(typeof(Severity), prival & 0x7),
                        Fac = (Facility)Enum.ToObject(typeof(Facility), prival >> 3),
                        Message = defMatch.Groups["MESSAGE"].Value.Trim(),
                    };
                    SentList.Add(sent);
                    UC.Send(msg, msg.Length, TargetIp, UdpPort);
                    localCounter++;
                    Thread.Sleep(Delay);
                }
            }
            return localCounter;
        }

    }
}

