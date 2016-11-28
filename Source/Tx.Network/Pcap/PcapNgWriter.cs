namespace Tx.Network
{
    using System;
    using System.IO;

    public sealed class PcapNgWriter : IObserver<byte[]>, IDisposable
    {
        private readonly Stream stream;

        private static readonly byte[] BlockTypeBytes = { 3, 0, 0, 0 };

        private readonly byte[] paddingBuffer = { 0, 0, 0, 0 };

        public PcapNgWriter(string filename)
        {
            this.stream = File.Create(filename);

            // Write Section Header Block as per http://xml2rfc.tools.ietf.org/cgi-bin/xml2rfc.cgi?url=https://raw.githubusercontent.com/pcapng/pcapng/master/draft-tuexen-opsawg-pcapng.xml&modeAsFormat=html/ascii&type=ascii#section_spb
            // To have produced files compatible with PcapNg format
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="e">The current notification information.</param>
        public void OnNext(byte[] e)
        {
            var length = 12 + (e.Length / 4 + 1) * 4;
            var padSize = length - 12 - e.Length;

            this.stream.Write(BlockTypeBytes, 0, BlockTypeBytes.Length);
            var size = BitConverter.GetBytes(length);
            this.stream.Write(size, 0, 4);
            this.stream.Write(e, 0, e.Length);

            // do padding
            if (padSize > 0)
            {
                this.stream.Write(this.paddingBuffer, 0, padSize);
            }

            this.stream.Write(size, 0, 4);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            this.stream.Flush();
            this.stream.Close();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }
    }

    //[global::Bond.Schema]
    //internal partial class BondBinaryEnvelope
    //{
    //    [global::Bond.Id(1)]
    //    public long ReceiveFileTimeUtc { get; set; }
    //    [global::Bond.Id(2)]
    //    public long OccurenceFileTimeUtc { get; set; }
    //    [global::Bond.Id(3)]
    //    public string PayloadId { get; set; }
    //    [global::Bond.Id(4)]
    //    public string Source { get; set; }
    //    [global::Bond.Id(5)]
    //    public string Protocol { get; set; }
    //    [global::Bond.Id(6)]
    //    public byte[] EventPayload { get; set; }
    //}

    //public sealed class PcapNgWriter : IObserver<IEnvelope>, IDisposable
    //{
    //    private readonly Stream stream;

    //    private readonly Serializer<CompactBinaryWriter<OutputBuffer>> serializer;

    //    private readonly OutputBuffer outputBuffer = new OutputBuffer();

    //    private readonly CompactBinaryWriter<OutputBuffer> writer1;

    //    private static readonly byte[] BlockTypeBytes = { 3, 0, 0, 0 };

    //    private readonly byte[] paddingBuffer = { 0, 0, 0, 0 };

    //    public PcapNgWriter(string filename)
    //    {
    //        this.stream = File.Create(filename);

    //        // Write Section Header Block as per http://xml2rfc.tools.ietf.org/cgi-bin/xml2rfc.cgi?url=https://raw.githubusercontent.com/pcapng/pcapng/master/draft-tuexen-opsawg-pcapng.xml&modeAsFormat=html/ascii&type=ascii#section_spb
    //        // To have produced files compatible with PcapNg format

    //        this.serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(BondBinaryEnvelope));
    //        this.writer1 = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
    //    }

    //    public void OnNext(IEnvelope e)
    //    {
    //        this.outputBuffer.Position = 0;

    //        var value = new BondBinaryEnvelope
    //        {
    //            EventPayload = e.Payload,
    //            OccurenceFileTimeUtc = e.OccurrenceTime.ToFileTime(),
    //            ReceiveFileTimeUtc = e.ReceivedTime.ToFileTime(),
    //            PayloadId = e.TypeId,
    //            Source = e.Source,
    //            Protocol = e.Protocol
    //        };

    //        this.serializer.Serialize(value, this.writer1);

    //        var length = 12 + ((this.outputBuffer.Data.Count) / 4 + 1) * 4;
    //        var padSize = length - 12 - this.outputBuffer.Data.Count;

    //        this.stream.Write(BlockTypeBytes, 0, BlockTypeBytes.Length);
    //        var size = BitConverter.GetBytes(length);
    //        this.stream.Write(size, 0, 4);
    //        this.stream.Write(this.outputBuffer.Data.Array, this.outputBuffer.Data.Offset, this.outputBuffer.Data.Count);

    //        // do padding
    //        if (padSize > 0)
    //        {
    //            this.stream.Write(this.paddingBuffer, 0, padSize);
    //        }

    //        this.stream.Write(size, 0, 4);
    //    }

    //    public void OnCompleted()
    //    {
    //        this.stream.Flush();
    //        this.stream.Close();
    //    }

    //    public void OnError(Exception e)
    //    {
    //    }

    //    public void Dispose()
    //    {
    //        this.stream.Dispose();
    //    }
    //}

    //[global::Bond.Schema]
    //internal partial class BondBinaryEnvelope
    //{
    //    [global::Bond.Id(1)]
    //    public long ReceiveFileTimeUtc { get; set; }
    //    [global::Bond.Id(2)]
    //    public long OccurenceFileTimeUtc { get; set; }
    //    [global::Bond.Id(3)]
    //    public string PayloadId { get; set; }
    //    [global::Bond.Id(4)]
    //    public string Source { get; set; }
    //    [global::Bond.Id(5)]
    //    public string Protocol { get; set; }
    //    [global::Bond.Id(6)]
    //    public byte[] EventPayload { get; set; }
    //}
}
