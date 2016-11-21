// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive;
    using System.Text;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public class BondJsonEnvelopeTypeMap : EnvelopeTypeMap
    {
        private readonly JsonSerializer jsonSerializer;

        public BondJsonEnvelopeTypeMap()
            : this(false)
        {            
        }

        public BondJsonEnvelopeTypeMap(bool handleTransportObject)
            : base(handleTransportObject)
        {
            var serializationSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
            };
            serializationSettings.Converters.Add(new IpAddressConverter());
            this.jsonSerializer = JsonSerializer.CreateDefault(serializationSettings);
        }

        protected override IReadOnlyDictionary<string, Func<byte[], object>> BuildDeserializers(Type outputType)
        {
            Func<byte[], object> jsonDeserializer = e => this.DeserializeJson(e, outputType);
            Func<byte[], object> bsonDeserializer = e => this.DeserializeBson(e, outputType);

            var deserializerMap = new Dictionary<string, Func<byte[], object>>(StringComparer.Ordinal)
            {
                { Protocol.Json, jsonDeserializer },
                { Protocol.Bson, bsonDeserializer }
            };

            if (outputType.IsBondStruct())
            {
                var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(outputType);

                deserializerMap.Add(Protocol.CompactBinaryV1, e => DeserializeCompactBinary(1, e, deserializer));
                deserializerMap.Add(Protocol.CompactBinaryV2, e => DeserializeCompactBinary(2, e, deserializer));
            }

            return deserializerMap;
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

        private object DeserializeJson(byte[] data, Type outputType)
        {
            using (var jsonTextReader = new JsonTextReader(new StringReader(Encoding.UTF8.GetString(data))))
            {
                return this.jsonSerializer.Deserialize(jsonTextReader, outputType);
            }
        }

        private object DeserializeBson(byte[] data, Type outputType)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var jsonTextReader = new BsonReader(memoryStream))
            {
                return this.jsonSerializer.Deserialize(jsonTextReader, outputType);
            }
        }
    }
}
