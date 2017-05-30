using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tx.Network.Tests
{
    [TestClass]
    public class UnitTestWireEncoding
    {
        [TestMethod]
        public void TestMethod1()
        {
            //bytes are in Network Order
            byte[] sampleUdpBytes = { 0x45, 0x00, 0x00, 0x4e, 0x70, 0x3a, 0x00, 0x00, 0x80, 0x11, 0xaa, 0x2a, 0x0a, 0x78, 0x85, 0x4b, 0x0a, 0x78, 0x85, 0xff, 0x00, 0x89, 0x00, 0x89, 0x00, 0x3a, 0x12, 0x55, 0x8a, 0x6a, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x46, 0x48, 0x46, 0x41, 0x45, 0x42, 0x45, 0x45, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x41, 0x41, 0x00, 0x00, 0x20, 0x00, 0x01 };
            byte[] sampleIpHeaderBytes = { 0x45, 0x00, 0x00, 0x4e, 0x70, 0x3a, 0x00, 0x00, 0x80, 0x11, 0xaa, 0x2a, 0x0a, 0x78, 0x85, 0x4b, 0x0a, 0x78, 0x85, 0xff };
            //byte[] sampleIpHeaderBytesNoCkSum = { 0x45, 0x00, 0x00, 0x4e, 0x70, 0x3a, 0x00, 0x00, 0x80, 0x11, 0x00, 0x00, 0x0a, 0x78, 0x85, 0x4b, 0x0a, 0x78, 0x85, 0xff };
            byte[] sampleIpCksumBytes = { 0xaa, 0x2a };
            byte[] sampleUdpCksumBytes = { 0x12, 0x55 };

            var ipPacket = PacketParser.Parse(DateTimeOffset.UtcNow, true, sampleUdpBytes, 0, sampleUdpBytes.Length);

            var testDatagram = ipPacket.ToUdpDatagram();

            //Preserved the ip checksum correctly in the object
            var IpVerify = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(sampleIpCksumBytes, 0));
            //Assert.AreEqual(testDatagram.PacketHeaderChecksum, IpVerify);

            //Preserved the udp checksum correctly in the object
            var UdpVerify = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(sampleUdpCksumBytes, 0));
            Assert.AreEqual(testDatagram.UdpDatagramHeader.UdpCheckSum, UdpVerify);

            //Ip header in transform from object to wire-bytes is correct
            //var IpHeaderVerify = testDatagram.PacketHeaderToWireBytes();
            //Assert.IsTrue(IpHeaderVerify.SequenceEqual(sampleIpHeaderBytes));

            ////checksum on header should be zero if header has the correct checksum in it.
            //var IpHeaderCk = NetworkTransformExtentions.GetInternetChecksum(IpHeaderVerify);
            //Assert.AreEqual(0, IpHeaderCk);

            //Udp check is correct
            var UdpCk = (ushort)IPAddress.NetworkToHostOrder((short)testDatagram.GetUdpCheckSum());
            Assert.AreEqual(UdpVerify, UdpCk);

            ////the whole datagram is right
            //var datagramCheck = testDatagram.ToWirebytes();
            //Assert.IsTrue(datagramCheck.SequenceEqual(sampleUdpBytes));
        }
    }
}
