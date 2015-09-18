// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Text;
    using System.Web.Script.Serialization;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;

    public sealed class GeneralPartitionableTypeMap : IPartitionableTypeMap<BinaryEnvelope, string>
    {
        private static readonly IEqualityComparer<string> comparer = StringComparer.OrdinalIgnoreCase;

        // Objects returned by transform call should not be null and be of System.Object type.
        private static readonly object defaultInstance = new { };

        private static readonly JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

        private readonly Dictionary<Type, Func<BinaryEnvelope, object>> transforms = new Dictionary<Type, Func<BinaryEnvelope, object>>();

        public IEqualityComparer<string> Comparer
        {
            get { return comparer; }
        }

        public string GetTypeKey(Type outputType)
        {
            string manifestId;

            try
            {
                manifestId = outputType.GetBondManifestIdentifier();
            }
            catch
            {
                manifestId = string.Empty;
            }

            return manifestId;
        }

        public Func<BinaryEnvelope, object> GetTransform(Type outputType)
        {
            Func<BinaryEnvelope, object> transform;
            this.transforms.TryGetValue(outputType, out transform);

            if (transform != null)
            {
                return transform;
            }

            Func<BinaryEnvelope, object> jsonDeserializer = e => DeserializeJson(e.EventPayload, outputType);

            var deserializerMap = new Dictionary<string, Func<BinaryEnvelope, object>>(StringComparer.OrdinalIgnoreCase)
            {
                { "JSON", jsonDeserializer }
            };

            if (outputType.IsBondStruct())
            {
                var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(outputType);

                deserializerMap.Add("BOND_V1", e => DeserializeCompactBinary(1, e.EventPayload, deserializer));
                deserializerMap.Add("BOND", e => DeserializeCompactBinary(2, e.EventPayload, deserializer));
            }

            transform = e => Transform(e, deserializerMap);

            this.transforms.Add(outputType, transform);

            return transform;
        }

        public Func<BinaryEnvelope, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetTime;
            }
        }

        public string GetInputKey(BinaryEnvelope envelope)
        {
            return envelope.PayloadId;
        }

        private static DateTimeOffset GetTime(BinaryEnvelope envelope)
        {
            var time = DateTimeOffset.FromFileTime(envelope.ReceiveFileTimeUtc);

            return time;
        }

        private static object Transform(
            BinaryEnvelope envelope,
            IDictionary<string, Func<BinaryEnvelope, object>> deserializerMap)
        {
            if (envelope.EventPayload == null)
            {
                return defaultInstance;
            }

            object deserializedObject;

            try
            {
                Func<BinaryEnvelope, object> transform;
                if (deserializerMap.TryGetValue(envelope.Protocol, out transform))
                {
                    deserializedObject = transform(envelope) ?? defaultInstance;
                }
                else
                {
                    deserializedObject = defaultInstance;
                }
            }
            catch
            {
                deserializedObject = defaultInstance;
            }

            return deserializedObject;
        }

        private static object DeserializeCompactBinary(
            ushort version,
            byte[] data,
            Deserializer<CompactBinaryReader<InputBuffer>> deserializer)
        {
            var inputStream = new InputBuffer(data);

            var reader = new CompactBinaryReader<InputBuffer>(inputStream, version);

            var outputObject = deserializer.Deserialize(reader);

            return outputObject;
        }

        private static object DeserializeJson(byte[] data, Type outputType)
        {
            var json = Encoding.UTF8.GetString(data);

            return javaScriptSerializer.Deserialize(json, outputType);
        }
    }
}
