// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using System.Collections.Generic;

    /// <summary>
    /// Custom EventSource Implementation that writes BinaryEnvelope information to ETW stream
    /// </summary>
    [EventSource(Name = "Tx-BinaryEventSource", Guid = "4f8f06bf-8261-4099-ae5f-07c54bbcfab3")]
    public sealed class BinaryEventSource : EventSource
    {
        private uint currentPackageId;
        private uint currentManifestPackageId;

        private readonly object writeChuckedBinaryPayloadGuard = new object();
        private readonly object writeChuckedManifestPayloadGuard = new object();

        // 12:00 midnight January 1, 1601 C.E. UTC.
        private readonly DateTime minDateTime = new DateTime(504911232000000000L, DateTimeKind.Utc);

        public const int MaxPayloadSize = 57 * 1024 - 118;

        /// <summary>
        /// Singleton instance of BondEtwWriter used for logging
        /// </summary>
        public static BinaryEventSource Log = new BinaryEventSource();

        /// <summary>
        /// Write an event's payload as an ETW event.
        /// </summary>
        /// <remarks>
        /// This method iternally check if the event cannot be saved as a single ETW event due to 64K size limitation and
        /// chunk it if needed.
        /// </remarks>
        /// <param name="occurenceTime">
        /// Occurrence time of the event as per source's system time
        /// </param>
        /// <param name="receiveTime">
        /// Receive time as per the system clock the Correlation Service machine (DateTime.UtcNow)
        /// </param>
        /// <param name="protocol">
        /// A protocol head - e.g. WCF
        /// </param>
        /// <param name="source">
        /// An event source. e.g. connector name
        /// </param>
        /// <param name="eventData">
        /// Event payload data.
        /// </param>
        /// <param name="manifestId">
        /// Identifer of the payload.
        /// </param>
        [NonEvent]
        public void Write(
            DateTime occurenceTime,
            DateTime receiveTime,
            string protocol,
            string source,
            byte[] eventData,
            string manifestId)
        {
            if (this.IsEnabled())
            {
                if (protocol == null)
                {
                    throw new ArgumentNullException("protocol");
                }

                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                if (eventData == null)
                {
                    throw new ArgumentNullException("eventData");
                }

                if (manifestId == null)
                {
                    throw new ArgumentNullException("manifestId");
                }

                // before 12:00 midnight January 1, 1601 C.E. UTC.
                if (occurenceTime.ToUniversalTime().Ticks < 504911232000000000L)
                {
                    occurenceTime = this.minDateTime;
                }

                // before 12:00 midnight January 1, 1601 C.E. UTC.
                if (receiveTime.ToUniversalTime().Ticks < 504911232000000000L)
                {
                    receiveTime = this.minDateTime;
                }

                this.WriteInternal(
                    occurenceTime,
                    receiveTime,
                    protocol,
                    source,
                    manifestId,
                    eventData);
            }
        }

        /// <summary>
        /// Write an event's payload as an ETW event.
        /// </summary>
        /// <remarks>
        /// This method iternally check if the event cannot be saved as a single ETW event due to 64K size limitation and
        /// chunk it if needed.
        /// </remarks>
        /// <param name="occurenceTime">
        /// Occurrence time of the event as per source's system time
        /// </param>
        /// <param name="receiveTime">
        /// Receive time as per the system clock the Correlation Service machine (DateTime.UtcNow)
        /// </param>
        /// <param name="protocol">
        /// A protocol head - e.g. WCF
        /// </param>
        /// <param name="source">
        /// An event source. e.g. connector name
        /// </param>
        /// <param name="manifestId">
        /// Identifer of the payload.
        /// </param>
        /// <param name="manifestData">
        /// Event payload data.
        /// </param>
        [NonEvent]
        public void WriteManifest(
            DateTime occurenceTime,
            DateTime receiveTime,
            string protocol,
            string source,
            string manifestId,
            string manifestData)
        {
            if (this.IsEnabled())
            {
                if (protocol == null)
                {
                    throw new ArgumentNullException("protocol");
                }

                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                if (manifestData == null)
                {
                    throw new ArgumentNullException("manifestData");
                }

                if (manifestId == null)
                {
                    throw new ArgumentNullException("manifestId");
                }

                // before 12:00 midnight January 1, 1601 C.E. UTC.
                if (occurenceTime.ToUniversalTime().Ticks < 504911232000000000L)
                {
                    occurenceTime = this.minDateTime;
                }

                // before 12:00 midnight January 1, 1601 C.E. UTC.
                if (receiveTime.ToUniversalTime().Ticks < 504911232000000000L)
                {
                    receiveTime = this.minDateTime;
                }

                this.WriteManifestInternal(
                    occurenceTime,
                    receiveTime,
                    protocol,
                    source,
                    manifestId,
                    manifestData);
            }
        }

        [NonEvent]
        private void WriteManifestInternal(
            DateTime occurenceTime,
            DateTime receiveTime,
            string inputProtocol,
            string source,
            string manifestId,
            string manifestData)
        {
            // Maximum record size is 64K for both system and user data, so counting 88 bytes for system data here as well
            var maxPayloadSize = (MaxPayloadSize / 2) - (manifestId.Length + source.Length + inputProtocol.Length) * 2;

            if (maxPayloadSize <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var occurenceFileTimeUtc = occurenceTime.ToFileTimeUtc();
            var receiveFileTimeUtc = receiveTime.ToFileTimeUtc();

            if (manifestData.Length <= maxPayloadSize)
            {
                this.WriteManifestPayload(
                    occurenceFileTimeUtc,
                    receiveFileTimeUtc,
                    inputProtocol,
                    source,
                    manifestId,
                    manifestData);
            }
            else
            {
                // User data for chunked event is 12 bytes greather than non-chunked event
                maxPayloadSize -= 12;

                List<string> chunks = new List<string>(manifestData.WholeChunks(maxPayloadSize));

                lock (this.writeChuckedManifestPayloadGuard)
                {
                    var packageId = unchecked(this.currentManifestPackageId++);
                    int i = 0;

                    foreach (string chunk in chunks)
                    {
                        this.WriteChunkedManifestPayload(
                            packageId,
                            occurenceFileTimeUtc,
                            receiveFileTimeUtc,
                            inputProtocol,
                            source,
                            manifestId,
                            chunks.Count,
                            i++,
                            chunk);
                    }
                }
            }
        }

        [NonEvent]
        private void WriteInternal(
            DateTime occurenceTime,
            DateTime receiveTime,
            string inputProtocol,
            string source,
            string manifestId,
            byte[] eventPayload)
        {
            // Maximum record size is 64K for both system and user data, so counting 88 bytes for system data here as well
            var maxPayloadSize = MaxPayloadSize - (manifestId.Length + source.Length + inputProtocol.Length) * 2;

            if (maxPayloadSize <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var occurenceFileTimeUtc = occurenceTime.ToFileTimeUtc();
            var receiveFileTimeUtc = receiveTime.ToFileTimeUtc();

            if (eventPayload.Length <= maxPayloadSize)
            {
                this.WriteBinaryPayload(
                    occurenceFileTimeUtc,
                    receiveFileTimeUtc,
                    inputProtocol,
                    source,
                    manifestId,
                    unchecked((uint)(eventPayload.Length)),
                    eventPayload);
            }
            else
            {
                // User data for chunked event is 12 bytes greather than non-chunked event
                maxPayloadSize -= 12;

                var chunks = eventPayload.Split(maxPayloadSize);

                lock (this.writeChuckedBinaryPayloadGuard)
                {
                    var packageId = unchecked(this.currentPackageId++);

                    for (uint i = 0; i < chunks.Length; i++)
                    {
                        this.WriteChunkedBinaryPayload(
                            packageId,
                            occurenceFileTimeUtc,
                            receiveFileTimeUtc,
                            inputProtocol,
                            source,
                            manifestId,
                            unchecked((uint)(chunks.Length)),
                            i,
                            unchecked((uint)(chunks[i].Length)),
                            chunks[i]);
                    }
                }
            }
        }

        [Event(1, Level = EventLevel.LogAlways, Message = "occurenceFileTimeUtc={0}, receiveFileTimeUtc={1}, protocol={2}, source={3}, manifestId={4}, eventPayloadLength={5}, eventPayload={6}")]
        public void WriteBinaryPayload(
            long occurenceFileTimeUtc,
            long receiveFileTimeUtc,
            string inputProtocol,
            string source,
            string manifestId,
            uint eventPayloadLength,
            byte[] eventPayload)
        {
            this.WriteEvent(
                1,
                occurenceFileTimeUtc,
                receiveFileTimeUtc,
                inputProtocol,
                source,
                manifestId,
                eventPayloadLength,
                eventPayload);
        }

        [Event(2, Level = EventLevel.LogAlways, Message = "packageId={0}, occurenceFileTimeUtc={1}, receiveFileTimeUtc={2}, protocol={3}, source={4}, manifestId={5}, chunk #{7} of {6} {8} bytes length")]
        public void WriteChunkedBinaryPayload(
            uint packageId, 
            long occurenceFileTimeUtc,
            long receiveFileTimeUtc,
            string inputProtocol,
            string source,
            string manifestId,
            uint chunkCount,
            uint currentChunkNumber,
            uint payloadLength,
            byte[] payload)
        {
            this.WriteEvent(
                2,
                packageId,
                occurenceFileTimeUtc,
                receiveFileTimeUtc,
                inputProtocol,
                source,
                manifestId,
                chunkCount,
                currentChunkNumber,
                payloadLength,
                payload);
        }

        [Event(3, Level = EventLevel.LogAlways, Message = "occurenceFileTimeUtc={0}, receiveFileTimeUtc={1}, protocol={2}, source={3}, manifestId={4}, manifestPayload={5}")]
        public void WriteManifestPayload(
            long occurenceFileTimeUtc,
            long receiveFileTimeUtc,
            string inputProtocol,
            string source,
            string manifestId,
            string manifestPayload)
        {
            this.WriteEvent(
                3,
                occurenceFileTimeUtc,
                receiveFileTimeUtc,
                inputProtocol,
                source,
                manifestId,
                manifestPayload);
        }

        [Event(4, Level = EventLevel.LogAlways, Message = "packageId={0}, occurenceFileTimeUtc={1}, receiveFileTimeUtc={2}, protocol={3}, source={4}, manifestId={5}, chunk #{7} of {6}, manifestPayload {8}")]
        public void WriteChunkedManifestPayload(
            uint packageId,
            long occurenceFileTimeUtc,
            long receiveFileTimeUtc,
            string inputProtocol,
            string source,
            string manifestId,
            int chunkCount,
            int currentChunkNumber,
            string manifestPayload)
        {
            this.WriteEvent(
                4,
                packageId,
                occurenceFileTimeUtc,
                receiveFileTimeUtc,
                inputProtocol,
                source,
                manifestId,
                chunkCount,
                currentChunkNumber,
                manifestPayload);
        }

        [Event(5, Level = EventLevel.Error, Message = "{0}")]
        public void Error(string error)
        {
            this.WriteEvent(5, error); 
        }
    }
}
