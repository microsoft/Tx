namespace Tests.Tx.BinaryEtw
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using global::Tx.Binary;
    using global::Tx.Bond;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GeneralPartitionableTypeMapTests
    {
        [TestMethod]
        public void DeserializeJson()
        {
            var typeMap = new GeneralPartitionableTypeMap();

            var typekey = typeMap.GetTypeKey(typeof(string));
            var envelope = new BinaryEnvelope
            {
                EventPayload = Encoding.UTF8.GetBytes(@"""A"""),
                PayloadId = "daf0be6e-da1e-5a6a-0d49-69782745c885",
                Protocol = "JSON"
            };

            var typekey2 = typeMap.GetInputKey(envelope);
            Assert.AreEqual(typekey, typekey2);

            var result = typeMap.GetTransform(typeof(string))(envelope);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void DeserializeBondV1()
        {
            var typeMap = new GeneralPartitionableTypeMap();

            var typekey = typeMap.GetTypeKey(typeof(TestBondClass));

            var envelope = new BinaryEnvelope
            {
                EventPayload = new byte[] { 41, 1, 65, 0 },
                PayloadId = "daf0be6e-da1e-5a6a-0d49-69782745c886",
                Protocol = "BOND_V1"
            };

            var typekey2 = typeMap.GetInputKey(envelope);
            Assert.AreEqual(typekey, typekey2);

            var result = typeMap.GetTransform(typeof(TestBondClass))(envelope);
        }

        [TestMethod]
        public void DeserializeMixedStream()
        {
            var input = new[]
            {
                new SimpleEnvelope("JSON", Guid.Parse("daf0be6e-da1e-5a6a-0d49-69782745c885"), Encoding.UTF8.GetBytes(@"""A""")), 
                new SimpleEnvelope("JSON", Guid.Parse("daf0be6e-da1e-5a6a-0d49-69782745c885"), new byte[0]), 
                new SimpleEnvelope("BOND_V1", new Guid("daf0be6e-da1e-5a6a-0d49-69782745c886"), new byte[] { 41, 1, 65, 0 }), 
            };

            IEnumerable<TestBondClass> testBondClassCollection;
            IEnumerable<string> stringCollection;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => input.ToObservable(),
                    typeof(GeneralPartitionableTypeMap));

                var testBondClassStream = playback.GetObservable<TestBondClass>();
                var stringStream = playback.GetObservable<string>();

                testBondClassCollection = playback.BufferOutput(testBondClassStream);
                stringCollection = playback.BufferOutput(stringStream);

                playback.Run();
            }

            var bondObjects = testBondClassCollection.ToArray();
            Assert.IsNotNull(bondObjects);
            Assert.AreEqual(1, bondObjects.Length);

            var strings = stringCollection.ToArray();
            Assert.IsNotNull(strings);
            Assert.AreEqual(1, strings.Length);
        }

        private class SimpleEnvelope : BinaryEnvelope
        {
            public SimpleEnvelope(string protocol, Guid manifestId, byte[] data)
            {
                this.Protocol = protocol;
                this.PayloadId = manifestId.ToString();
                this.EventPayload = data;
            }
        }

        [global::Bond.Schema]
        [Guid("daf0be6e-da1e-5a6a-0d49-69782745c886")]
        public partial class TestBondClass
        {
            [global::Bond.Id(1)]
            public string EventId { get; set; }
        }
    }
}
