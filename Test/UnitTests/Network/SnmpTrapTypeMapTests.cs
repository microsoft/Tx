namespace Tests.Tx.Network
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Reactive;

    using global::Tx.Network;
    using global::Tx.Network.Snmp;
    using global::Tx.Network.Snmp.Dynamic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SnmpTrapTypeMapTests
    {
        private UdpDatagram fakeTrapUdp;

        [TestInitialize]
        public void TestInitialize()
        {
            var integerVarBind = new VarBind(
                new ObjectIdentifier("1.3.6.1.4.1.1.1.1"),
                5L,
                new Asn1TagInfo(Asn1Tag.Integer, ConstructType.Primitive, Asn1Class.Universal));

            var sysUpTime = new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"),
                506009u,
                new Asn1TagInfo(Asn1SnmpTag.TimeTicks));

            var trapVb = new VarBind(new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0"),
                new ObjectIdentifier("1.3.6.1.4.1.500.12"),
                new Asn1TagInfo(Asn1Tag.ObjectIdentifier, ConstructType.Primitive, Asn1Class.Universal));

            var extraneousVb = new VarBind(new ObjectIdentifier("1.3.6.1.6.3.1.1.42.42.42.0"),
                8938ul,
                new Asn1TagInfo(Asn1SnmpTag.Counter64));

            var packet = new SnmpDatagramV2C(
                DateTimeOffset.MinValue, 
                "1.1.1.1",
                new SnmpHeader(SnmpVersion.V2C, "Community"),
                new[] { sysUpTime, trapVb, integerVarBind, extraneousVb },
                PduType.SNMPv2Trap,
                50000,
                SnmpErrorStatus.NoError,
                0);

            var encoded = packet.ToSnmpEncodedByteArray();

            this.fakeTrapUdp = new UdpDatagram
            {
                UdpData = encoded.AsByteArraySegment(),
                PacketHeader = new IpPacketHeader(IPAddress.Parse("1.1.1.1"), IPAddress.Parse("2.2.2.2"), false, 1, 1, 1, 1, 1, 1, 1, 1, 1)
            };
        }

        [TestMethod]
        public void TestUnattributedClassReturnsDefaultKey()
        {
            var typeMap = new SnmpTrapTypeMap();
            var key = typeMap.GetTypeKey(typeof(TrapTypeMapTests.UnmarkedTrap));

            Assert.IsNotNull(key);
            Assert.AreEqual(default(ObjectIdentifier), key);
        }

        [TestMethod]
        public void TestUnattributedClassReturnsNullTransform()
        {
            var typeMap = new SnmpTrapTypeMap();
            var transform = typeMap.GetTransform(typeof(TrapTypeMapTests.UnmarkedTrap));

            Assert.IsNull(transform);
        }

        [TestMethod]
        public void TestFakeTrapTransform()
        {
            var typeMap = new SnmpTrapTypeMap();

            var transform = typeMap.GetTransform(typeof(TrapTypeMapTests.FakeTrap));

            Assert.IsNotNull(transform);

            var receivedTime = DateTimeOffset.UtcNow;
            this.fakeTrapUdp.ReceivedTime = receivedTime;

            var env = new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "", "", "", null, this.fakeTrapUdp);
            var key = typeMap.GetInputKey(env);
            var transformedOutput = transform(env) as TrapTypeMapTests.FakeTrap;

            Assert.IsNotNull(transformedOutput);
            Assert.AreEqual(506009u, transformedOutput.SysUpTime);
            Assert.AreEqual(5L, transformedOutput.Integer);
            Assert.IsNotNull(transformedOutput.Objects);
            Assert.AreEqual(4, transformedOutput.Objects.Count);
            Assert.AreEqual(IPAddress.Parse("1.1.1.1"), transformedOutput.SourceAddress);
            Assert.AreEqual(receivedTime, transformedOutput.ReceivedTime);

            var varbinds = transformedOutput.Objects;
            var extraneous = varbinds.FirstOrDefault(vb => vb.Oid.Equals(new ObjectIdentifier("1.3.6.1.6.3.1.1.42.42.42.0")));

            Assert.IsNotNull(extraneous);
            Assert.AreNotEqual(default(VarBind), extraneous);
            Assert.AreEqual(8938ul, extraneous.Value);
        }

        [TestMethod]
        public void TestNullTrap()
        {
            var typeMap = new SnmpTrapTypeMap();

            var transform = typeMap.GetTransform(typeof(TrapTypeMapTests.FakeTrap));

            Assert.IsNotNull(transform);

            var transformedOutput = transform(null) as TrapTypeMapTests.FakeTrap;

            Assert.IsNull(transformedOutput);
        }

        [TestMethod]
        public void TestFakeTrapStringIpTransform()
        {
            var typeMap = new SnmpTrapTypeMap();

            var transform = typeMap.GetTransform(typeof(TrapTypeMapTests.FakeTrapStringIp));

            Assert.IsNotNull(transform);

            var env = new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "", "", "", null, this.fakeTrapUdp);
            var key = typeMap.GetInputKey(env);
            var transformedOutput = transform(env) as TrapTypeMapTests.FakeTrapStringIp;

            Assert.IsNotNull(transformedOutput);
            Assert.AreEqual("1.1.1.1", transformedOutput.SourceAddress);
        }

        [TestMethod]
        public void TestFakeTrapInputKey()
        {
            var typeMap = new SnmpTrapTypeMap();
            var env = new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "", "", "", null, this.fakeTrapUdp);
            var inputKey = typeMap.GetInputKey(env);

            Assert.AreEqual(new ObjectIdentifier("1.3.6.1.4.1.500.12"), inputKey);

            var typeKey = typeMap.GetTypeKey(typeof(TrapTypeMapTests.FakeTrap));

            Assert.AreEqual(inputKey, typeKey);
        }

        [SnmpTrap("1.3.6.1.4.1.500.12")]
        internal class FakeTrap2
        {
            [SnmpOid("1.3.6.1.4.1.562.29.6.2.2")]
            public byte[] Property { get; set; }

            [SnmpOid("1.3.6.1.4.1.562.29.6.2.2")]
            public string StringProperty { get; set; }
        }

        [SnmpTrap("1.3.6.1.4.1.500.12")]
        internal class FakeTrap3
        {
            [SnmpOid("1.3.6.1.4.1.562.29.6.1.1.1.6")]
            public SimpleEnum EnumProperty { get; set; }
        }

        public enum SimpleEnum
        {
            A = 0,
            B = 1,
            C = 2,
        };

        [SnmpTrap("1.3.6.1.4.1.500.12")]
        internal class FakeTrap
        {
            [SnmpOid("1.3.6.1.2.1.1.3.0")]
            public uint SysUpTime { get; set; }

            [SnmpOid("1.3.6.1.4.1.1.1.1")]
            public long Integer { get; set; }

            [IpAddress]
            public IPAddress SourceAddress { get; set; }

            [NotificationObjects]
            public ReadOnlyCollection<VarBind> Objects { get; set; }

            [Timestamp]
            public DateTimeOffset ReceivedTime { get; set; }
        }

        [SnmpTrap("1.3.6.1.4.1.500.12")]
        internal class FakeTrapStringIp
        {
            [IpAddress]
            public string SourceAddress { get; set; }
        }

        internal class UnmarkedTrap
        {
            [SnmpOid("1.3.6.1.2.1.1.3.0")]
            public uint SysUpTime { get; set; }
        }
    }
}
