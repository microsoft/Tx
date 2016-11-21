namespace Tx.Network.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tx.Network.Snmp;

    [TestClass]
    public class SnmpSimpleDatagramTest
    {
        //private SnmpDatagram testSnmpDatagram =
        //    new SnmpDatagram(PduType.GetRequest,
        //        SnmpVersion.V2C,
        //        "test",
        //        1,
        //        SnmpErrorStatus.NoError,
        //        1,
        //        new VarBind[]{
        //            new VarBind(new ObjectIdentifier("1.2.4.6.22.3.5.6.7.0.1"), (uint)2314, new Asn1TagInfo(Asn1SnmpTag.Counter32)),
        //            new VarBind(new ObjectIdentifier("1.2.4.6.22.3.5.6.7.0.2"), (uint)234, new Asn1TagInfo(Asn1SnmpTag.Gauge32)),
        //            new VarBind(new ObjectIdentifier("1.2.4.6.22.3.5.6.7.0.3"), (ulong)2114, new Asn1TagInfo(Asn1SnmpTag.Counter64)),
        //            new VarBind(new ObjectIdentifier("1.2.4.6.22.3.5.6.7.0.4"), (uint)2310, new Asn1TagInfo(Asn1SnmpTag.Counter32)),
        //        });

        //[TestMethod]
        //public void SnmpSimpleDatagramInitilizationTest()
        //{
        //    var testObject = new SnmpSimpleDatagram(testSnmpDatagram.ToSnmpEncodedByteArray().AsByteArraySegment());

        //    Assert.IsNotNull(testObject);
        //    Assert.AreEqual(testObject.Community, "test");
        //    Assert.AreEqual(testObject.Version, SnmpVersion.V2C);
        //    Assert.AreEqual(testObject.RequestId, 1);
        //    Assert.AreEqual(testObject.ErrorIndex, 1);
        //    Assert.AreEqual(testObject.ErrorStatus, SnmpErrorStatus.NoError);
        //    Assert.AreEqual(testObject.VarBinds.Count, 4);
        //    Assert.AreEqual(testObject.VarBinds[0], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.1", (uint)2314));
        //    Assert.AreEqual(testObject.VarBinds[1], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.2", (uint)234));
        //    Assert.AreEqual(testObject.VarBinds[2], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.3", (ulong)2114));
        //    Assert.AreEqual(testObject.VarBinds[3], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.4", (uint)2310));
        //}


        //[TestMethod]
        //public void SnmpSimpleDatagramEncodingTest()
        //{
        //    var originalEncoding = testSnmpDatagram.ToSnmpEncodedByteArray();
        //    var testObject = new SnmpSimpleDatagram(testSnmpDatagram);
        //    Assert.IsTrue(originalEncoding.SequenceEqual(testObject.EncodeToAsn1ByteArray()));
        //}

        //[TestMethod]
        //public void SnmpSimpleDatagramSearchFirstSubOidWithTest()
        //{
        //    var testObject = new SnmpSimpleDatagram(testSnmpDatagram);
        //    KeyValuePair<string, object> testParam;
        //    Assert.IsTrue(testObject.SearchFirstSubOidWith("1.2.4.6.22.3.5.6.7.0", out testParam));
        //    Assert.AreEqual(testParam, new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.1", (uint)2314));
        //    Assert.IsFalse(testObject.SearchFirstSubOidWith("1.2.4.6.122.3.15.6.7.10", out testParam));
        //}

        //[TestMethod]
        //public void SnmpSimpleDatagramSearchLastSubOidWithTest()
        //{
        //    var testObject = new SnmpSimpleDatagram(testSnmpDatagram);
        //    KeyValuePair<string, object> testParam;
        //    Assert.IsTrue(testObject.SearchLastSubOidWith("1.2.4.6.22.3.5.6.7.0", out testParam));
        //    Assert.AreEqual(testParam, new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.4", (uint)2310));
        //    Assert.IsFalse(testObject.SearchFirstSubOidWith("1.2.4.6.122.3.15.6.7.10", out testParam));
        //}

        //[TestMethod]
        //public void SnmpSimpleDatagramGetAllOidWithTest()
        //{
        //    var testObject = new SnmpSimpleDatagram(testSnmpDatagram);
        //    var testParam = testObject.GetAllOidsStartingWith("1.2.4.6.22.3.5.6.7.0");
        //    Assert.AreEqual(testParam.Count, 4);
        //    Assert.AreEqual(testParam[0], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.1", (uint)2314));
        //    Assert.AreEqual(testParam[1], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.2", (uint)234));
        //    Assert.AreEqual(testParam[2], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.3", (ulong)2114));
        //    Assert.AreEqual(testParam[3], new KeyValuePair<string, object>("1.2.4.6.22.3.5.6.7.0.4", (uint)2310));

        //    testParam = testObject.GetAllOidsStartingWith("1.2.4.6.122.3.15.6.7.10");
        //    Assert.AreEqual(testParam.Count, 0);
        //}
    }
}
