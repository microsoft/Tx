using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Network.Snmp;

namespace Tx.Network.Tests
{
    [TestClass]
    public class SnmpDatagramUnitTest
    {
        private static IList<byte[]> testDeviceResponseCollection;

        private static byte[] V1trapBytes;

        /// <summary>
        /// Tests the initi.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize]
        public static void TestIniti(TestContext testContext)
        {
            testDeviceResponseCollection = new List<byte[]>();
            foreach (var line in File.ReadAllLines(".\\DeviceResponseData.dat"))
            {
                testDeviceResponseCollection.Add(line.Split(',').Select(x => (byte)Convert.ToInt32(x)).ToArray());
            }

            V1trapBytes = "48,129,135,2,1,0,4,6,112,117,98,108,105,99,164,122,6,11,6,43,43,43,43,43,43,43,43,43,43,64,4,127,0,0,1,2,1,6,2,2,5,62,67,4,8,6,62,180,48,88,48,26,6,18,43,6,6,6,6,6,6,6,6,6,2,1,1,56,67,77,77,77,2,4,33,10,3,66,48,29,6,14,43,6,6,6,6,6,6,6,6,6,6,6,7,55,4,11,7,7,8,10,19,9,39,1,43,2,0,48,27,6,14,43,6,6,6,6,6,6,6,6,2,1,4,155,55,4,9,83,3,3,3,3,3,3,3,3".Split(',').Select(x => (byte)Convert.ToInt32(x)).ToArray();
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
                    var snmpPacket = device.ToSnmpDatagram() as SnmpDatagramV2C;
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
        /// Encodes the decoded data without error.
        /// </summary>
        [TestMethod]
        public void SnmpTrapV1DecodedDataWithoutError()
        {
            try
            {
                var datagram = V1trapBytes.ToSnmpDatagram();
                Assert.IsTrue(datagram.Header.Version == SnmpVersion.V1);
                //Assert.IsTrue(datagram.PduV1.PduType == PduType.Trap);
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
            long mSec;
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
            Assert.IsTrue(mSec < 1250, "Parser running slower than designed, Time Taken :" + mSec.ToString());
#else
              Assert.IsTrue(mSec < 600, "Parser running slower than designed, Time Taken :" + mSec.ToString());
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

            long mSec;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < 500; i++)
                {
                    foreach (var packet in packets)
                    {
                        (packet as SnmpDatagramV2C).ToSnmpEncodedByteArray();
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
            Assert.IsTrue(mSec < 1250, "Parser running slower than designed, Time Taken :" + mSec.ToString());
#else
              Assert.IsTrue(mSec < 600, "Parser running slower than designed, Time Taken :" + mSec.ToString());
#endif
        }

        [TestMethod]
        public void Asn1ObjectIdentifierTest()
        {
            var objectIdentifier1 = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            var objectIdentifier2 = new ObjectIdentifier("1.3.6.1.2.1.1.1.1.1");
            var objectIdentifier3 = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");

            Assert.IsTrue(objectIdentifier1.IsSubOid(objectIdentifier3));
            Assert.IsTrue(objectIdentifier1.IsSubOid(new ObjectIdentifier("1.3.6.1.2.1.1")));
            Assert.IsTrue(!objectIdentifier1.IsSubOid(objectIdentifier2));

            Assert.IsFalse(default(ObjectIdentifier).IsSubOid(objectIdentifier1));
            Assert.IsFalse(objectIdentifier1.IsSubOid(default(ObjectIdentifier)));

            Assert.IsTrue(default(ObjectIdentifier).IsSubOid(default(ObjectIdentifier)));
        }
    }
}
