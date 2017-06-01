namespace Tx.Network
{
    using System;
    using System.IO;

    internal sealed class PcapNgWriter : IObserver<byte[]>, IDisposable
    {
        private static readonly byte[] BlockTypeBytes = { 3, 0, 0, 0 };

        private readonly byte[] paddingBuffer = { 0, 0, 0, 0 };

        private Stream stream;

        public PcapNgWriter(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            this.stream = File.Create(filename);

            // Write Section Header Block as per http://xml2rfc.tools.ietf.org/cgi-bin/xml2rfc.cgi?url=https://raw.githubusercontent.com/pcapng/pcapng/master/draft-tuexen-opsawg-pcapng.xml&modeAsFormat=html/ascii&type=ascii#section_spb
            // To have produced files to be compliant with PcapNg format
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var length = 12 + (value.Length / 4 + 1) * 4;
            var padSize = length - 12 - value.Length;

            this.stream.Write(BlockTypeBytes, 0, BlockTypeBytes.Length);
            var size = BitConverter.GetBytes(length);
            this.stream.Write(size, 0, 4);
            this.stream.Write(value, 0, value.Length);

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
            this.Dispose();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Flush();
                this.stream.Dispose();
                this.stream = null;
            }
        }
    }
}
