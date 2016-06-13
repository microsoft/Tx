namespace Tx.Network.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tx.Network.Snmp;

    [TestClass]
    public class SnmpTrapTest
    {

        private static SnmpDatagram testSnmpDatagram =
            new SnmpDatagram(PduType.GetRequest,
                SnmpVersion.V2C,
                "test",
                1,
                SnmpErrorStatus.NoError,
                1,
                new VarBind[]{
                    new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"), 2314u, new Asn1TagInfo(Asn1SnmpTag.TimeTicks)),
                    new VarBind(new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0"), new ObjectIdentifier("1.3.6.1.2.1.1.3.0.23"), new Asn1TagInfo(Asn1Tag.ObjectIdentifier)),
                });

        private static byte[] V1trapBytes;

        /// <summary>
        /// Tests the initi.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize]
        public static void TestIniti(TestContext testContext)
        {
           V1trapBytes = "48,129,135,2,1,0,4,6,112,117,98,108,105,99,164,122,6,11,6,43,43,43,43,43,43,43,43,43,43,64,4,127,0,0,1,2,1,6,2,2,5,62,67,4,8,6,62,180,48,88,48,26,6,18,43,6,6,6,6,6,6,6,6,6,2,1,1,56,67,77,77,77,2,4,33,10,3,66,48,29,6,14,43,6,6,6,6,6,6,6,6,6,6,6,7,55,4,11,7,7,8,10,19,9,39,1,43,2,0,48,27,6,14,43,6,6,6,6,6,6,6,6,2,1,4,155,55,4,9,83,3,3,3,3,3,3,3,3".Split(',').Select(x => (byte)Convert.ToInt32(x)).ToArray();
        }

        [TestMethod]
        public void SnmpTrapV2CInitilizationTest()
        {
            var testObject = new SnmpTrapV2C(testSnmpDatagram.ToSnmpEncodedByteArray());

            Assert.IsNotNull(testObject);
            Assert.AreEqual(testObject.SysUpTime, 2314u);
            Assert.AreEqual(testObject.TrapOid, "1.3.6.1.2.1.1.3.0.23");
            Assert.AreEqual(testObject.Header.Community, "test");
            Assert.AreEqual(testObject.Header.Version, SnmpVersion.V2C);
            Assert.AreEqual(testObject.PduV2c.RequestId, 1);
            Assert.AreEqual(testObject.PduV2c.ErrorIndex, 1);
            Assert.AreEqual(testObject.PduV2c.ErrorStatus, SnmpErrorStatus.NoError);
            Assert.AreEqual(testObject.PduV2c.VarBinds.Count, 2);
            Assert.AreEqual(testObject.PduV2c.VarBinds[0], new KeyValuePair<string, object>("1.3.6.1.2.1.1.3.0", 2314u));
            Assert.AreEqual(testObject.PduV2c.VarBinds[1], new KeyValuePair<string, object>("1.3.6.1.6.3.1.1.4.1.0", new ObjectIdentifier("1.3.6.1.2.1.1.3.0.23")));
        }

        [TestMethod]
        public void SnmpTrapV2CBadInitilizationTest()
        {
            try
            {
                var testObject = new SnmpTrapV2C(V1trapBytes);
                Assert.Fail("Should throw Exception for bad Version");
            }
            catch (InvalidDataException ivD)
            {
                Assert.IsTrue(ivD.Message.Contains("Not a Valid V2c Trap"));
            }
        }

        [TestMethod]
        public void SnmpTrapV1CInitilizationTest()
        {
            var testObject = new SnmpTrapV1(V1trapBytes);

            Assert.IsNotNull(testObject);
            Assert.AreEqual(testObject.SysUpTime, 134626996u);
            Assert.AreEqual(testObject.TrapOid, new ObjectIdentifier("0.6.43.43.43.43.43.43.43.43.43.43"));
            Assert.AreEqual(testObject.Header.Community, "public");
            Assert.AreEqual(testObject.Header.Version, SnmpVersion.V1);
            Assert.AreEqual(testObject.PduV1.AgentAddress.ToString(), "127.0.0.1");
            Assert.AreEqual(testObject.PduV1.Enterprise, new ObjectIdentifier("0.6.43.43.43.43.43.43.43.43.43.43"));
            Assert.AreEqual(testObject.PduV1.GenericV1Trap, Snmp.Asn1Types.GenericTrap.EnterpriseSpecific);
            Assert.AreEqual(testObject.PduV1.VarBinds.Count, 3);
            Assert.AreEqual(testObject.PduV1.TimeStamp, 134626996u);
            Assert.AreEqual(testObject.PduV1.SpecificTrap, 1342);
        }

        [TestMethod]
        public void SnmpTrapV1CBadInitilizationTest()
        {
            try
            {
                var testObject = new SnmpTrapV1(testSnmpDatagram.ToSnmpEncodedByteArray());
                Assert.Fail("Should throw Exception for bad Version");
            }
            catch (InvalidDataException ivD)
            {
                Assert.IsTrue(ivD.Message.Contains("Not a Valid V1 Trap"));
            }
        }
    }
}
