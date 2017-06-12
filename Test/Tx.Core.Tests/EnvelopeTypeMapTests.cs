namespace Tests.Tx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnvelopeTypeMapTests
    {
        [TestMethod]
        public void DeserializeMixedStream()
        {
            var integerIdentifier = typeof(int).GetTypeIdentifier();
            var stringIdentifier = typeof(string).GetTypeIdentifier();

            var input = new[]
            {
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "A", "1", stringIdentifier, null, "Hello"),
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "B", "2", "", null, "Hello"),
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, "B", "2", integerIdentifier, null, 10),
            };

            IEnumerable<int> intCollection;
            IEnumerable<string> stringCollection;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => input.ToObservable(),
                    typeof(EnvelopeTypeMap));

                var intStream = playback.GetObservable<int>();
                var stringStream = playback.GetObservable<string>();

                intCollection = playback.BufferOutput(intStream);
                stringCollection = playback.BufferOutput(stringStream);

                playback.Run();
            }

            var bondObjects = intCollection.ToArray();
            Assert.IsNotNull(bondObjects);
            Assert.AreEqual(1, bondObjects.Length);
            Assert.AreEqual(10, bondObjects[0]);

            var strings = stringCollection.ToArray();
            Assert.IsNotNull(strings);
            Assert.AreEqual(1, strings.Length);
            Assert.AreEqual("Hello", strings[0]);
        }

        [TestMethod]
        public void InvalidInputData()
        {
            var input = new[]
            {
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, null, null, null, null, null),
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, null, null, typeof(string).GetTypeIdentifier(), null, null),
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, null, null, null, null, "A"),
                new Envelope(DateTimeOffset.MinValue, DateTimeOffset.MinValue, null, null, typeof(int).GetTypeIdentifier(), null, null),
            };

            IEnumerable<string> stringCollection;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(() => input.ToObservable(), typeof(EnvelopeTypeMap));

                var stringStream = playback.GetObservable<string>();

                stringCollection = playback.BufferOutput(stringStream);

                playback.Run();
            }

            var strings = stringCollection.ToArray();
            Assert.IsNotNull(strings);
            Assert.AreEqual(0, strings.Length);
        }
    }
}
