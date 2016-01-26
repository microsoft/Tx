namespace Tx.Network.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tx.Network.Snmp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Asn1BEREncoderUnitTest
    {
        [TestMethod]
        public void SnmpEncodeToClassConstructTypeTest()
        {
            byte[] testBit = new byte[] { 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(testBit.EncodeClassConstructType(0, Asn1Class.Universal, ConstructType.Primitive, (byte)Asn1Tag.Integer) == 1);
            Assert.IsTrue(testBit[0]==2);
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
    }
}
