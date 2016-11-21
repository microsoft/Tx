namespace Tx.Network.UnitTests
{
    using Tx.Network.Snmp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Asn1BERDecoderUnitTest
    {
        [TestMethod]
        public void SnmpDecodeToClassConstructTypeTest()
        {
            byte testBit = (byte)48;
            var decodedType = testBit.DecodeToClassConstructType();
            Assert.IsTrue(decodedType.Asn1ClassType == Asn1Class.Universal);
            Assert.IsTrue(decodedType.Asn1ConstructType == ConstructType.Constructed);
            Assert.IsTrue(decodedType.Asn1SnmpTagType == Asn1SnmpTag.NotSnmpData);
            Assert.IsTrue(decodedType.Asn1TagType == Asn1Tag.Sequence);

            testBit = (byte)2;
            decodedType = testBit.DecodeToClassConstructType();
            Assert.IsTrue(decodedType.Asn1ClassType == Asn1Class.Universal);
            Assert.IsTrue(decodedType.Asn1ConstructType == ConstructType.Primitive);
            Assert.IsTrue(decodedType.Asn1SnmpTagType == Asn1SnmpTag.NotSnmpData);
            Assert.IsTrue(decodedType.Asn1TagType == Asn1Tag.Integer);

            testBit = (byte)4;
            decodedType = testBit.DecodeToClassConstructType();
            Assert.IsTrue(decodedType.Asn1ClassType == Asn1Class.Universal);
            Assert.IsTrue(decodedType.Asn1ConstructType == ConstructType.Primitive);
            Assert.IsTrue(decodedType.Asn1SnmpTagType == Asn1SnmpTag.NotSnmpData);
            Assert.IsTrue(decodedType.Asn1TagType == Asn1Tag.OctetString);

            testBit = (byte)65;
            decodedType = testBit.DecodeToClassConstructType();
            Assert.IsTrue(decodedType.Asn1ClassType == Asn1Class.Application);
            Assert.IsTrue(decodedType.Asn1ConstructType == ConstructType.Primitive);
            Assert.IsTrue(decodedType.Asn1SnmpTagType == Asn1SnmpTag.Counter32);
            Assert.IsTrue(decodedType.Asn1TagType == Asn1Tag.NotAsn1Data);
        }

        [TestMethod]
        public void SnmpIntegerDecodeTest()
        {
            byte[] bytes = new byte[] { 7, 102, 0 };
            int retVal = bytes.ReadInteger(0, 2);
            Assert.IsTrue(retVal == 1894);

            bytes = new byte[] { 1, 0, 0 };
            retVal = bytes.ReadInteger(0, 1);
            Assert.IsTrue(retVal == 1);

            bytes = new byte[] { 0, 0, 0 };
            retVal = bytes.ReadInteger(0, 1);
            Assert.IsTrue(retVal == 0);
        }

        [TestMethod]
        public void SnmpReadLengthDecodeTest()
        {
            byte[] bytes = new byte[] { 7,102, 2, 1,1 };
            int retVal;
            bytes.ReadLength(0, out retVal);
            Assert.IsTrue(retVal == 7);
        }
    }
}
