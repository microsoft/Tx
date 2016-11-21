namespace Tx.Bond
{
    using System;
    using System.Reactive;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Core;

    public class BondCompactBinaryTransformerBuilder : ITransformerBuilder<IEnvelope>
    {
        public Func<TIn, IEnvelope> Build<TIn>()
        {
            if (!typeof(TIn).IsBondStruct())
            {
                return null;
            }

            return new BondCompactBinaryWriter<TIn>(true).Transform;
        }

        internal sealed class BondCompactBinaryWriter<T>
        {
            private readonly string manifestId;

            private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;

            private readonly OutputBuffer outputBuffer = new OutputBuffer();

            private readonly CompactBinaryWriter<OutputBuffer> writer;

            private readonly string protocol;

            public BondCompactBinaryWriter(bool preferCompactBinaryV1OverV2)
            {
                var type = typeof(T);

                if (!type.IsBondStruct())
                {
                    throw new NotSupportedException();
                }

                this.manifestId = type.GetTypeIdentifier();
                this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(type);
                this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
                this.protocol = preferCompactBinaryV1OverV2 ? Protocol.CompactBinaryV1 : Protocol.CompactBinaryV2;
            }

            public IEnvelope Transform(T value)
            {
                var now = DateTime.UtcNow;

                this.outputBuffer.Position = 0;

                this.serializer.Serialize(value, this.writer);

                var envelope = new Envelope(now, now, this.protocol, null, this.manifestId, this.outputBuffer.Data.ToByteArray(), null);

                return envelope;
            }
        }
    }
}