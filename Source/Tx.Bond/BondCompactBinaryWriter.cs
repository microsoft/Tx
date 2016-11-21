namespace Tx.Bond
{
    //using System;
    //using System.Reactive;
    //using System.Reactive.V2;

    //using global::Bond;
    //using global::Bond.IO.Safe;
    //using global::Bond.Protocols;

    //public class BondCompactBinaryTransformerBuilder : ITransformBuilder<IEnvelope>
    //{
    //    public Func<TIn, IEnvelope> Build<TIn>()
    //    {
    //        if (!typeof(TIn).IsBondStruct())
    //        {
    //            return null;
    //        }

    //        return new BondCompactBinaryWriter<TIn>(true).Transform;
    //    }
    //}

    //public class BondCompactBinaryWriter<T>
    //{
    //    private readonly string manifestId;

    //    private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;

    //    private readonly OutputBuffer outputBuffer = new OutputBuffer();

    //    private readonly CompactBinaryWriter<OutputBuffer> writer;

    //    private readonly string protocol;

    //    public BondCompactBinaryWriter(bool preferCompactBinaryV1OverV2)
    //    {
    //        var type = typeof(T);

    //        if (!type.IsBondStruct())
    //        {
    //            throw new NotSupportedException();
    //        }

    //        this.manifestId = type.GetTypeIdentifier();
    //        this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(type);
    //        this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
    //        this.protocol = preferCompactBinaryV1OverV2 ? Protocol.CompactBinaryV1 : Protocol.CompactBinaryV2;
    //    }

    //    public IEnvelope Transform(T value)
    //    {
    //        var now = DateTime.UtcNow;

    //        this.outputBuffer.Position = 0;

    //        this.serializer.Serialize(value, this.writer);

    //        var envelope = new Envelope(now, now, this.protocol, null, this.manifestId, this.outputBuffer.Data.ToByteArray(), null);

    //        return envelope;
    //    }
    //}

    //public class BondCompactBinaryWriter<T> : IObserver<T>
    //{
    //    private readonly IObserver<IEnvelope> next;

    //    private readonly string manifestId;

    //    private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;

    //    private readonly OutputBuffer outputBuffer = new OutputBuffer();

    //    private readonly CompactBinaryWriter<OutputBuffer> writer;

    //    private readonly string protocol;

    //    public BondCompactBinaryWriter(IObserver<IEnvelope> next, bool preferCompactBinaryV1OverV2)
    //    {
    //        if (next == null)
    //        {
    //            throw new ArgumentNullException("next");
    //        }

    //        var type = typeof(T);

    //        if (!type.IsBondStruct())
    //        {
    //            throw new NotSupportedException();
    //        }

    //        this.next = next;
    //        this.manifestId = type.GetTypeIdentifier();
    //        this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(type);
    //        this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
    //        this.protocol = preferCompactBinaryV1OverV2 ? Protocol.CompactBinaryV1 : Protocol.CompactBinaryV2;
    //    }

    //    public void OnNext(T value)
    //    {
    //        var now = DateTime.UtcNow;

    //        this.outputBuffer.Position = 0;

    //        this.serializer.Serialize(value, this.writer);

    //        var envelope = new Envelope(now, now, this.protocol, null, this.manifestId, this.outputBuffer.Data.ToByteArray(), null);

    //        this.next.OnNext(envelope);
    //    }

    //    public void OnError(Exception error)
    //    {
    //        this.next.OnError(error);
    //    }

    //    public void OnCompleted()
    //    {
    //        this.next.OnCompleted();
    //    }
    //}
}