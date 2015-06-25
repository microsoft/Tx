// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Linq;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;

    public static class BondProtocol
    {
        public const string CompactBinaryV1 = "BOND_V1";
        public const string CompactBinaryV2 = "BOND";
    }

    public class BondEtwWriter<T>
    {
        private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;

        private readonly string manifestId;

        public BondEtwWriter()
        {
            if (!typeof(T).IsBondType())
            {
                throw new NotSupportedException("T type should be valid Bond type");
            }

            this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(T));
            this.manifestId = typeof(T).GetBondManifestIdentifier();
        }

        public void Write(T eventType, DateTime occurenceTimeUtc, DateTime receiveTimeUtc, string source)
        {
            var outputBuffer = new OutputBuffer();

            this.serializer.Serialize(eventType, new CompactBinaryWriter<OutputBuffer>(outputBuffer, 1));

            BinaryEventSource.Log.Write(
                occurenceTimeUtc, 
                receiveTimeUtc,
                BondProtocol.CompactBinaryV1, 
                source, 
                outputBuffer.Data.ToArray(),
                this.manifestId);
        }
    }
}
