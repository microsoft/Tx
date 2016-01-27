namespace Tx.Network.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
                Assert.Fail("Failed to Decode data " + ex.ToString());
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
                Assert.Fail("Encoding/Decoding Failed " + ex.ToString());
            }
        }

        /// <summary>
        /// Decoders the performance test.
        /// </summary>
        [TestMethod]
        public void SnmpDecoderPerformanceTest()
        {
            long mSec = 0;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    foreach (var device in testDeviceResponseCollection)
                    {
                        device.ToSnmpDatagram();
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to Decode data " + ex.ToString());
            }
            finally
            {
                mSec = sw.ElapsedMilliseconds;
                sw.Stop();
            }

            #if DEBUG
              Assert.IsTrue(mSec < 1000, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #else
              Assert.IsTrue(mSec < 400, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #endif
        }

        /// <summary>
        /// Encoders the performance test.
        /// </summary>
        [TestMethod]
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
                for (int i = 0; i < 100; i++)
                {
                    foreach (var packet in packets)
                    {
                        packet.ToSnmpEncodedByteArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to Encode data " + ex.ToString());
            }
            finally
            {
                mSec = sw.ElapsedMilliseconds;
                sw.Stop();
            }

            #if DEBUG
              Assert.IsTrue(mSec < 1000, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #else
              Assert.IsTrue(mSec < 400, "Parser running slower than designed, Time Taken :" + mSec.ToString());
            #endif
        }
    }
}
