using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tx.Network.Syslogs;

namespace Tx.Network.Tests
{
    /// <summary>
    /// Simulates sending Syslogs over the network.
    /// </summary>
    internal class TxSyslogSender
    {
        //public int Counter;
        public CancellationToken Cancel { get; set; }

        public List<SimpleTxSyslog> SentList { get; }
        public TxSyslogSender()
        {
            this.Cancel = Task.Factory.CancellationToken;
            this.SentList = new List<SimpleTxSyslog>();
        }

        public Task<int> StartSendAsync(string sourceIp, string targetIp, int udpPort, TimeSpan delay, TimeSpan duration, List<string> source)
        {
            return Task.Factory.StartNew<int>(() => this.Send(sourceIp, targetIp, udpPort, delay, duration, source), this.Cancel);
        }

        public int Send(string sourceIp, string targetIp, int udpPort, TimeSpan delay, TimeSpan duration, List<string> source)
        {
            var start = DateTime.UtcNow;
            var localCounter = 0;
            using (var client = new UdpClient(new IPEndPoint(IPAddress.Parse(sourceIp), 0)))
            {
                while (DateTime.UtcNow < start.Add(duration))
                {
                    var txtMsg = source[localCounter % source.Count];
                    var msg = Encoding.ASCII.GetBytes(txtMsg);
                    var defMatch = SyslogParser.DefaultParser.Match(txtMsg);
                    var privalMatch = defMatch.Groups["PRIVAL"].Value.Trim();
                    var prival = int.Parse(privalMatch);
                    var sent = new SimpleTxSyslog()
                    {
                        Sev = (Severity)Enum.ToObject(typeof(Severity), prival & 0x7),
                        Fac = (Facility)Enum.ToObject(typeof(Facility), prival >> 3),
                        Message = defMatch.Groups["MESSAGE"].Value.Trim(),
                    };
                    this.SentList.Add(sent);
                    client.SendAsync(msg, msg.Length, targetIp, udpPort).Wait();
                    localCounter++;
                    Thread.Sleep(delay);
                }
            }
            return localCounter;
        }

    }
}

