using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tx.Bond.Tests
{
    [TestClass]
    public class EnvelopeTypeMapTests
    {
        [TestMethod]
        public void DeserializeJson()
        {
            var typeMap = new BondJsonEnvelopeTypeMap();

            var typekey = typeMap.GetTypeKey(typeof(string));
            var envelope = new SimpleEnvelope(Protocol.Json, new Guid("daf0be6e-da1e-5a6a-0d49-69782745c885"), Encoding.UTF8.GetBytes(@"""A"""));

            var typekey2 = typeMap.GetInputKey(envelope);
            Assert.AreEqual(typekey, typekey2);

            var result = typeMap.GetTransform(typeof(string))(envelope);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void DeserializeBondV1()
        {
            var typeMap = new BondJsonEnvelopeTypeMap();

            var typekey = typeMap.GetTypeKey(typeof(TestBondClass));

            var envelope = new SimpleEnvelope(Protocol.CompactBinaryV1, new Guid("daf0be6e-da1e-5a6a-0d49-69782745c886"), new byte[] { 41, 1, 65, 0 });

            var typekey2 = typeMap.GetInputKey(envelope);
            Assert.AreEqual(typekey, typekey2);

            var result = typeMap.GetTransform(typeof(TestBondClass))(envelope);
        }

        [TestMethod]
        public void DeserializeMixedStream()
        {
            var input = new[]
            {
                new SimpleEnvelope(Protocol.Json, Guid.Parse("daf0be6e-da1e-5a6a-0d49-69782745c885"), Encoding.UTF8.GetBytes(@"""A""")), 
                new SimpleEnvelope(Protocol.Json, Guid.Parse("daf0be6e-da1e-5a6a-0d49-69782745c885"), new byte[0]), 
                new SimpleEnvelope(Protocol.CompactBinaryV1, new Guid("daf0be6e-da1e-5a6a-0d49-69782745c886"), new byte[] { 41, 1, 65, 0 }), 
            };

            IEnumerable<TestBondClass> testBondClassCollection;
            IEnumerable<string> stringCollection;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => input.ToObservable(),
                    typeof(BondJsonEnvelopeTypeMap));

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

        private class SimpleEnvelope : Envelope
        {
            public SimpleEnvelope(string protocol, Guid manifestId, byte[] data)
                : base(DateTimeOffset.MinValue, DateTimeOffset.MinValue, protocol, string.Empty, manifestId.ToString(), data, null)
            {
            }
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
