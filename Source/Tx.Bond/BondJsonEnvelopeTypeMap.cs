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

    public class BondJsonEnvelopeTypeMap : EnvelopeTypeMap
    {
        protected readonly JavaScriptSerializer JsonSerializer;

        public BondJsonEnvelopeTypeMap()
            : this(false, JsonTransformBuilder.DefaultSerializer)
        {            
        }

        public BondJsonEnvelopeTypeMap(bool handleTransportObject, JavaScriptSerializer serializer)
            : base(handleTransportObject)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.JsonSerializer = serializer;
        }

        protected override IReadOnlyDictionary<string, Func<byte[], object>> BuildDeserializers(Type outputType)
        {
            Func<byte[], object> jsonDeserializer = e => this.DeserializeJson(e, outputType);

            var deserializerMap = new Dictionary<string, Func<byte[], object>>(StringComparer.Ordinal)
            {
                { Protocol.Json, jsonDeserializer },
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
            var json = Encoding.UTF8.GetString(data);

            return this.JsonSerializer.Deserialize(json, outputType);
        }
    }
}
