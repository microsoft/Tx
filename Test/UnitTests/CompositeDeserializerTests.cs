namespace Tests.Tx
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CompositeDeserializerTests
    {
        [TestMethod]
        public void DeserializeIntoMultipleTargets()
        {
            var cache = new List<Timestamped<object>>();

            var deserializer = new CompositeDeserializer<Envelope>(
                    Observer.Create<Timestamped<object>>(item => cache.Add(item)),
                    new EnvelopeTestTypeMap(),
                    new EnvelopeTestTypeMap2(),
                    new EnvelopeTestTypeMap3())
            {
                EndTime = DateTime.MaxValue
            };

            deserializer.AddKnownType(typeof(string));
            deserializer.AddKnownType(typeof(Envelope));

            deserializer.OnNext(new Envelope { Data = "data", Timestamp = DateTimeOffset.UtcNow });

            Assert.AreEqual(2, cache.Count);
            Assert.IsInstanceOfType(cache[0].Value, typeof(string));
            Assert.IsInstanceOfType(cache[1].Value, typeof(Envelope));
        }

        internal sealed class EnvelopeTestTypeMap3 : IPartitionableTypeMap<Envelope, string>
        {
            public Func<Envelope, object> GetTransform(Type outputType)
            {
                return envelope => envelope.Data;
            }

            public Func<Envelope, DateTimeOffset> TimeFunction
            {
                get
                {
                    return envelope => envelope.Timestamp;
                }
            }

            public string GetTypeKey(Type outputType)
            {
                return null;
            }

            public string GetInputKey(Envelope evt)
            {
                return null;
            }

            public IEqualityComparer<string> Comparer
            {
                get
                {
                    return StringComparer.OrdinalIgnoreCase;
                }
            }
        }

        internal sealed class EnvelopeTestTypeMap : ITypeMap<Envelope>
        {
            public Func<Envelope, DateTimeOffset> TimeFunction
            {
                get
                {
                    return envelope => envelope.Timestamp;
                }                
            }

            public Func<Envelope, object> GetTransform(Type outputType)
            {
                return envelope => envelope.Data;
            }
        }

        internal sealed class EnvelopeTestTypeMap2 : ITypeMap<Envelope>
        {
            public Func<Envelope, DateTimeOffset> TimeFunction
            {
                get
                {
                    return envelope => envelope.Timestamp;
                }
            }

            public Func<Envelope, object> GetTransform(Type outputType)
            {
                return envelope => envelope;
            }
        }

        internal sealed class Envelope
        {
            public string Data;

            public DateTimeOffset Timestamp;
        }
    }
}
