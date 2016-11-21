namespace Tests.Tx.Network
{
    using global::Tx.Network;
    using global::Tx.Network.Syslogs;
    using System.Net;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UdpReceiverTests
    {
        [TestMethod]
        public void Construct()
        {
            using (new UdpReceiver("127.128.1.1"))
            {
            }
        }

        [TestMethod]
        public void Run_10s_10ms()
        {
            var duration = TimeSpan.FromSeconds(10);
            var delay = TimeSpan.FromMilliseconds(10);

            RunCompareSendReceive(duration, delay);
        }
        private void RunCompareSendReceive(TimeSpan duration, TimeSpan delay)
        {
            ReceiveTxSyslog txReceiver = new ReceiveTxSyslog(new IPEndPoint(IPAddress.Parse(TxSyslogTestSettings.TargetIP), TxSyslogTestSettings.TargetPort), 10);
            txReceiver.RunCount();
            var txRecdObs = txReceiver.ObserveTx();

            var receivedList = new List<Syslog>();
            txRecdObs
                .Subscribe<Syslog>(j => receivedList.Add(j));

            TxSyslogSender sysSend = new TxSyslogSender();
            var sendTask = sysSend.StartSendAsync(TxSyslogTestSettings.SourceIP,
                TxSyslogTestSettings.TargetIP,
                TxSyslogTestSettings.TargetPort,
                delay,
                duration,
                TxSyslogTestSettings.MessageList);

            var counter = sendTask.Result;

            Assert.AreEqual(counter, txReceiver.Counter);

            for (int c = 0; c < txReceiver.Counter; c++)
            {
                Assert.AreEqual(sysSend.SentList[c].Fac, receivedList[c].LogFacility);
                Assert.AreEqual(sysSend.SentList[c].Sev, receivedList[c].LogSeverity);
                Assert.AreEqual(TxSyslogTestSettings.SourceIP, receivedList[c].SourceIpAddress);
                Assert.AreEqual(sysSend.SentList[c].Message.ToLowerInvariant(), receivedList[c].Message.ToLowerInvariant());
                Console.Out.WriteLine(receivedList[c].Message);
                Console.Out.WriteLine(sysSend.SentList[c].Message);
            }
        }
    }
    internal sealed class ReceiveTxSyslog : IDisposable
    {
        public int Counter;
        public UdpReceiver Receiver { get; set; }
        public ReceiveTxSyslog(IPEndPoint listener, uint receivercount)
        {
            this.Receiver = new UdpReceiver(listener, receivercount);
        }
        public IDisposable RunCount()
        {
            return this.Receiver.Select(i => new SyslogParser().Parse(i)).Subscribe(s =>
            {
                if (s != null)
                {
                    this.Counter++;
                }
            });
        }
        public IObservable<Syslog> ObserveTx()
        {
            return this.Receiver.Select(i => new SyslogParser().Parse(i));
        }

        public void Dispose()
        {
            this.Receiver.Dispose();
        }
    }
}
