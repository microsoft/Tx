// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;
    using System.Reactive;
    using System.Globalization;

    /// <summary>
    /// The load bond type.
    /// </summary>
    public class BondTypeMap : IPartitionableTypeMap<BinaryEnvelope, string>
    {
        StringComparer _comparer = StringComparer.Create(CultureInfo.InvariantCulture, false);
        Dictionary<Type, Func<BinaryEnvelope, object>> _transforms = new Dictionary<Type, Func<BinaryEnvelope, object>>();
        public IEqualityComparer<string> Comparer
        {
            get { return _comparer; }
        }

        public string GetTypeKey(Type outputType)
        {
            return outputType.GetBondManifestIdentifier();
        }

        public Func<BinaryEnvelope, object> GetTransform(Type outputType)
        {
            Func<BinaryEnvelope, object> transform = null;
            _transforms.TryGetValue(outputType, out transform);

            if (transform != null)
                return transform;

            var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(outputType);
            var defaultInstance = Activator.CreateInstance(outputType);
            transform = e => GetBondObject(e, deserializer, defaultInstance);
            _transforms.Add(outputType, transform);

            return transform;
        }
        public Func<BinaryEnvelope, DateTimeOffset> TimeFunction
        {
            get
            {
                return e =>
                    DateTime.FromFileTimeUtc(e.ReceiveFileTimeUtc);
            }
        }

        public string GetInputKey(BinaryEnvelope envelope)
        {
            return envelope.PayloadId;
        }

        private static object GetBondObject(BinaryEnvelope envelope, Deserializer<CompactBinaryReader<InputBuffer>> deserializer, object defaultInstance)
        {
            var inputStream = new InputBuffer(envelope.EventPayload);

            var version = string.Equals(envelope.Protocol, BondProtocol.CompactBinaryV1, StringComparison.Ordinal)
                ? (ushort)1
                : (ushort)2;

            var reader = new CompactBinaryReader<InputBuffer>(inputStream, version);

            object outputObject;

            try
            {
                outputObject = deserializer.Deserialize(reader);
            }
            catch (Exception exception)
            {
                outputObject = defaultInstance;
                BinaryEventSource.Log.Error("Error trying to deserialize payload for " + envelope.PayloadId + ", error: " + exception);
            }

            return outputObject;
        }
    }
}
