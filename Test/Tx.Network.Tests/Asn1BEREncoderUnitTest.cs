using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Network.Snmp;

namespace Tx.Network.Tests
{
    [TestClass]
    public class Asn1BEREncoderUnitTest
    {
        [TestMethod]
        public void SnmpEncodeToClassConstructTypeTest()
        {
            byte[] testBit = new byte[] { 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(testBit.EncodeClassConstructType(0, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.Integer) == 1);
            Assert.IsTrue(testBit[0] == 2);
        }

        [TestMethod]
        public void SnmpIntegerEncodeTest()
        {
            byte[] bytes = new byte[] { 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(bytes.EncodeInteger(0, 1) == 3);
            Assert.IsTrue(bytes[0] == 2);
            Assert.IsTrue(bytes[1] == 1);
            Assert.IsTrue(bytes[2] == 1);
        }

        [TestMethod]
        public void SnmpReadLengthEncoderTest()
        {
            byte[] bytes = new byte[] { 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(bytes.EncodeLength(0, 1) == 1);
            Assert.IsTrue(bytes[0] == 1);
        }

        [TestMethod]
        public void TestTimeTicksTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            506009u, new Asn1TagInfo(Asn1SnmpTag.TimeTicks));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(506009u, (uint)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1UInt32EncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            506009u, new Asn1TagInfo(Asn1SnmpTag.UInt32));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(506009u, (uint)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1Counter64EncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            (ulong)50600900, new Asn1TagInfo(Asn1SnmpTag.Counter64));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual((ulong)50600900, (ulong)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1IntegerEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            506009979999L, new Asn1TagInfo(Asn1Tag.Integer));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(506009979999L, (long)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1NegativeIntegerEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            -506009979999L, new Asn1TagInfo(Asn1Tag.Integer));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(-506009979999L, (long)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1SmallIntegerEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            1, new Asn1TagInfo(Asn1Tag.Integer));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(1, (long)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1GaugeEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            506009u, new Asn1TagInfo(Asn1SnmpTag.Gauge));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(506009u, (uint)snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1IpAdressEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            new System.Net.IPAddress(123453), new Asn1TagInfo(Asn1SnmpTag.IpAddress));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual("61.226.1.0", snmpPack.VarBinds[0].Value.ToString());
        }

        [TestMethod]
        public void Asn1NullEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.IsNull(snmpPack.VarBinds[0].Value);
        }

        [TestMethod]
        public void Asn1NullAndNotNullEncodingTest()
        {
            var oidWithNull = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"));
            var oidWithoutNull = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"),
           1, new Asn1TagInfo(Asn1Tag.Integer));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { oidWithNull, oidWithoutNull },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.IsNull(snmpPack.VarBinds[0].Value);
            Assert.AreEqual(1, (long)snmpPack.VarBinds[1].Value);
        }

        [TestMethod]
        public void Asn1BigVarBindEncodingTest()
        {
            var varBinds = new[] {
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0")),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 1, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 13, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 143, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 122, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 13456, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 1334, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 1444, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 13, new Asn1TagInfo(Asn1Tag.Integer)),
               new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.1"), 10, new Asn1TagInfo(Asn1Tag.Integer)),
            };

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                varBinds,
                PduType.SNMPv2Trap,
                509000,
                SnmpErrorStatus.TooBig,
                10000);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.IsNull(snmpPack.VarBinds[0].Value);
            Assert.AreEqual(1, (long)snmpPack.VarBinds[1].Value);
        }

        [TestMethod]
        public void Asn1StringEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            "TestString", new Asn1TagInfo(Asn1Tag.OctetString));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);


            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual("TestString", snmpPack.VarBinds[0].Value.ToString());
        }

        [TestMethod]
        public void Asn1ObjectIdentifierEncodingTest()
        {
            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
            new ObjectIdentifier("1.3.6.1.2.1.1.1.1.1"), new Asn1TagInfo(Asn1Tag.ObjectIdentifier));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue,
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();
            var snmpPack = encoded.ToSnmpDatagram();

            Assert.AreEqual(new ObjectIdentifier("1.3.6.1.2.1.1.1.1.1"), (ObjectIdentifier)snmpPack.VarBinds[0].Value);
        }
    }
}
