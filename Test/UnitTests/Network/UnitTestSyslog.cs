
namespace Tx.Network.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reactive.Linq;
    using Tx.Network;

    [TestClass]
    public class UnitTestSyslog
    {
        [TestMethod]
        public void Run_1min_10ms()
        {
            var duration = TimeSpan.FromSeconds(60);
            var delay = TimeSpan.FromMilliseconds(10);

            Run(duration, delay);
        }
        [TestMethod]
        public void Run_10s_10ms()
        {
            var duration = TimeSpan.FromSeconds(10);
            var delay = TimeSpan.FromMilliseconds(10);

            Run(duration, delay);
        }
        [TestMethod]
        public void Run_10m_100ms()
        {
            var duration = TimeSpan.FromSeconds(600);
            var delay = TimeSpan.FromMilliseconds(100);

            Run(duration, delay);
        }
        [TestMethod]
        public void Run_SendReceive_1m_100ms()
        {
            var duration = TimeSpan.FromSeconds(60);
            var delay = TimeSpan.FromMilliseconds(100);
            RunCompareSendReceive(duration, delay);
        }
        [TestMethod]
        public void Run_SendReceive_10m_100ms()
        {
            var duration = TimeSpan.FromSeconds(600);
            var delay = TimeSpan.FromMilliseconds(100);
            RunCompareSendReceive(duration, delay);
        }
        private void Run(TimeSpan duration, TimeSpan delay)
        {
            ReceiveTxSyslog recTxSys = new ReceiveTxSyslog(new IPEndPoint(IPAddress.Parse(TxSyslogTestSettings.TargetIP), TxSyslogTestSettings.TargetPort), 10);
            recTxSys.RunCount();
            TxSyslogSender sendTxSys = new TxSyslogSender();
            var sendTask = sendTxSys.StartSendAsync(TxSyslogTestSettings.SourceIP,
                 TxSyslogTestSettings.TargetIP,
                 TxSyslogTestSettings.TargetPort,
                 delay,
                 duration,
                 TxSyslogTestSettings.MessageList);
            var count = sendTask.Result;
            Assert.AreEqual(count, recTxSys.Counter);
            Console.Out.WriteLine(" {0},{1} ", count, recTxSys.Counter);
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
                Assert.AreEqual(sysSend.SentList[c].Fac,receivedList[c].LogFacility);
                Assert.AreEqual(sysSend.SentList[c].Sev, receivedList[c].LogSeverity);
                Assert.AreEqual(TxSyslogTestSettings.SourceIP, receivedList[c].SourceIpAddress.ToString());
                Assert.AreEqual(TxSyslogTestSettings.TargetIP, receivedList[c].DestinationIpAddress.ToString());
                Assert.AreEqual(TxSyslogTestSettings.TargetPort, receivedList[c].DestinationPort);
                Assert.AreEqual(sysSend.SentList[c].Message.ToLowerInvariant(),receivedList[c].Message.ToLowerInvariant());
                Console.Out.WriteLine(receivedList[c].Message);
                Console.Out.WriteLine(sysSend.SentList[c].Message);
            }
        }
    }
}