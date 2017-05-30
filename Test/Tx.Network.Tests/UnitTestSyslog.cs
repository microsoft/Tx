using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Network.Syslogs;

namespace Tx.Network.Tests
{
    [TestClass]
    public class UnitTestSyslog
    {
        [TestMethod]
        public void TestBuildSyslogsSyslog()
        {

            var someDateTimeOffset = DateTimeOffset.UtcNow;
            var anyIpAddress = "127.0.0.1";
            var anySeverity = Severity.Alert;
            var anyFacility = Facility.Syslog;
            var anyMessage = "This is a syslog";
            var tmpDict = new Dictionary<string, string>();
            tmpDict.Add("somekey", "somevalue");
            var anyRoDictionary = new ReadOnlyDictionary<string, string>(tmpDict);

            var sys = new Syslog(someDateTimeOffset, anyIpAddress, anySeverity, anyFacility, anyMessage, anyRoDictionary);

            Assert.AreEqual(someDateTimeOffset, sys.ReceivedTime);
            Assert.AreEqual(anyIpAddress, sys.SourceIpAddress);
            Assert.AreEqual(anySeverity, sys.LogSeverity);
            Assert.AreEqual(anyFacility, sys.LogFacility);
            Assert.AreEqual(anyMessage, sys.Message);
            Assert.AreEqual(tmpDict["somekey"], sys.NamedCollectedMatches["somekey"]);
        }

        [TestMethod]
        public void TestBuildSyslogParser()
        {

            var sysparser = new SyslogParser();

            Assert.IsNotNull(sysparser);

            var anyRegex = new Regex(@"\<(?<PRIVAL>\d+?)\>\s*(?<MESSAGE>.+)");

            var sysparserWithRegex = new SyslogParser(anyRegex);

            Assert.IsNotNull(sysparserWithRegex);
        }

        [TestMethod]
        public void TestSyslogParserParsing()
        {
            var testprival = "<140>";
            var severity = (Severity)Enum.ToObject(typeof(Severity), 140 & 0x7);
            var facility = (Facility)Enum.ToObject(typeof(Facility), 140 >> 3);

            var testmessage =  "A message with any pri-val";
            var testString = testprival + testmessage;

            var udData = new ArraySegment<byte>(Encoding.ASCII.GetBytes(testString));

            var pHeader = new IpPacketHeader(
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("127.0.0.1"),
                false,
                4,
                0,
                0,
                (ushort)(udData.Array.Length + 20 + 8),
                0,
                0,
                0,
                255,
                0
                );
            

            var udHeader = new UdpDatagramHeader(16, 16, (ushort)udData.Array.Length, 0);
            UdpDatagram ud = new UdpDatagram()
            {
                UdpDatagramHeader = udHeader,
                UdpData = udData,
                PacketHeader = pHeader,
                ReceivedTime = DateTimeOffset.UtcNow
            };

            var sysparser = new SyslogParser();
            var sys = sysparser.Parse(ud);

            Assert.IsNotNull(sys);
            Assert.AreEqual(testmessage, sys.Message);
            Assert.AreEqual(severity, sys.LogSeverity);
            Assert.AreEqual(facility, sys.LogFacility);
            Assert.AreEqual(testmessage, sys.NamedCollectedMatches["MESSAGE"]);
        }

        [TestMethod]
        public void TestSyslogCustomParserParsing()
        {
            var testprival = "<140>";
            var severity = (Severity)Enum.ToObject(typeof(Severity), 140 & 0x7);
            var facility = (Facility)Enum.ToObject(typeof(Facility), 140 >> 3);

            var testmessage = "A message with any pri-val";
            var testString = testprival + testmessage;

            var udData = new ArraySegment<byte>(Encoding.ASCII.GetBytes(testString));

            var pHeader = new IpPacketHeader(
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("127.0.0.1"),
                false,
                4,
                0,
                0,
                (ushort)(udData.Array.Length + 20 + 8),
                0,
                0,
                0,
                255,
                0
                );


            var udHeader = new UdpDatagramHeader(16, 16, (ushort)udData.Array.Length, 0);
            UdpDatagram ud = new UdpDatagram()
            {
                UdpDatagramHeader = udHeader,
                UdpData = udData,
                PacketHeader = pHeader,
                ReceivedTime = DateTimeOffset.UtcNow
            };

            var customParser = new Regex(@"(?<WithAny>with\sany)", RegexOptions.ExplicitCapture);
            var sysparser = new SyslogParser(customParser);
            var sys = sysparser.Parse(ud);

            Assert.IsNotNull(sys);
            Assert.AreEqual(testmessage, sys.Message);
            Assert.AreEqual(severity, sys.LogSeverity);
            Assert.AreEqual(facility, sys.LogFacility);
            Assert.AreEqual("with any", sys.NamedCollectedMatches["WithAny"]);
        }

    }
}