namespace Tx.Network.UnitTests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tx.Network.Snmp;

    [TestClass]
    public class SnmpSimpleTrapV2CTest
    {
        //private SnmpDatagram testSnmpDatagram =
        //    new SnmpDatagram(PduType.GetRequest,
        //        SnmpVersion.V2C,
        //        "test",
        //        1,
        //        SnmpErrorStatus.NoError,
        //        1,
        //        new VarBind[]{
        //            new VarBind(new ObjectIdentifier("1.3.6.1.2.1.1.3.0"), 2314u, new Asn1TagInfo(Asn1SnmpTag.TimeTicks)),
        //            new VarBind(new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0"), new ObjectIdentifier("1.3.6.1.2.1.1.3.0.23"), new Asn1TagInfo(Asn1Tag.ObjectIdentifier)),
        //        });

        //[TestMethod]
        //public void SnmpSimpleTrapV2CInitilizationTest()
        //{
        //    var testObject = new SnmpSimpleTrapV2C(testSnmpDatagram.ToSnmpEncodedByteArray().AsByteArraySegment());

        //    Assert.IsNotNull(testObject);
        //    Assert.AreEqual(testObject.SysUpTime, 2314u);
        //    Assert.AreEqual(testObject.TrapOid, "1.3.6.1.2.1.1.3.0.23");
        //    Assert.AreEqual(testObject.Community, "test");
        //    Assert.AreEqual(testObject.Version, SnmpVersion.V2C);
        //    Assert.AreEqual(testObject.RequestId, 1);
        //    Assert.AreEqual(testObject.ErrorIndex, 1);
        //    Assert.AreEqual(testObject.ErrorStatus, SnmpErrorStatus.NoError);
        //    Assert.AreEqual(testObject.VarBinds.Count, 2);
        //    Assert.AreEqual(testObject.VarBinds[0], new KeyValuePair<string, object>("1.3.6.1.2.1.1.3.0", 2314u));
        //    Assert.AreEqual(testObject.VarBinds[1], new KeyValuePair<string, object>("1.3.6.1.6.3.1.1.4.1.0", new ObjectIdentifier("1.3.6.1.2.1.1.3.0.23")));
        //}
    }
}
