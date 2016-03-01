namespace Tx.Network.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Tx.Network.Snmp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SnmpDatagramUnitTest
    {
        private static IList<byte[]> testDeviceResponseCollection;

        /// <summary>
        /// Tests the initi.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize]
        public static void TestIniti(TestContext testContext)
        {
            testDeviceResponseCollection = new List<byte[]>();
            foreach (var line in File.ReadAllLines(".\\Network\\DeviceResponseData.dat"))
            {
                testDeviceResponseCollection.Add(line.Split(',').Select(x => (byte)Convert.ToInt32(x)).ToArray());
            }
        }

        /// <summary>
        /// Decodes the without error.
        /// </summary>
        [TestMethod]
        public void SnmpDecodeWithoutError()
        {
            try
            {
                foreach (var device in testDeviceResponseCollection)
                {
                   device.ToSnmpDatagram();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to Decode data " + ex);
            }
        }

        /// <summary>
        /// Encodes the decoded data without error.
        /// </summary>
        [TestMethod]
        public void SnmpEncodeDecodedDataWithoutError()
        {
            try
            {
                foreach (var device in testDeviceResponseCollection)
                {
                    SnmpDatagram snmpPacket = device.ToSnmpDatagram();
                    byte[] encodedData = snmpPacket.ToSnmpEncodedByteArray();

                    //Check if encoding is correct by decoding that back
                    encodedData.ToSnmpDatagram();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Encoding/Decoding Failed " + ex);
            }
        }

        /// <summary>
        /// Decoders the performance test.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SnmpDecoderPerformanceTest()
        {
            long mSec = 0;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    foreach (var device in testDeviceResponseCollection)
                    {
                        device.ToSnmpDatagram();
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to Decode data " + ex);
            }
            finally
            {
                mSec = sw.ElapsedMilliseconds;
                sw.Stop();
            }

            #if DEBUG
              Assert.IsTrue(mSec < 1050, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #else
              Assert.IsTrue(mSec < 500, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #endif
        }

        /// <summary>
        /// Encoders the performance test.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SnmpEncoderPerformanceTest()
        {
            IList<SnmpDatagram> packets = new List<SnmpDatagram>();
            foreach (var device in testDeviceResponseCollection)
            {
                packets.Add(device.ToSnmpDatagram());
            }

            long mSec = 0;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < 500; i++)
                {
                    foreach (var packet in packets)
                    {
                        packet.ToSnmpEncodedByteArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to Encode data " + ex);
            }
            finally
            {
                mSec = sw.ElapsedMilliseconds;
                sw.Stop();
            }

            #if DEBUG
              Assert.IsTrue(mSec < 1000, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #else
              Assert.IsTrue(mSec < 500, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #endif
        }

        [TestMethod]
        public void Asn1ObjectIdentifierTest()
        {
            var ObjectIdentifier1 = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            var ObjectIdentifier2 = new ObjectIdentifier("1.3.6.1.2.1.1.1.1.1");
            var ObjectIdentifier3 = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");

            Assert.IsTrue(ObjectIdentifier1.IsSubOid(ObjectIdentifier3));
            Assert.IsTrue(ObjectIdentifier1.IsSubOid(new ObjectIdentifier("1.3.6.1.2.1.1")));
            Assert.IsTrue(!ObjectIdentifier1.IsSubOid(ObjectIdentifier2));

            Assert.IsFalse(default(ObjectIdentifier).IsSubOid(ObjectIdentifier1));
            Assert.IsFalse(ObjectIdentifier1.IsSubOid(default(ObjectIdentifier)));

            Assert.IsTrue(default(ObjectIdentifier).IsSubOid(default(ObjectIdentifier)));
        }
    }
}
