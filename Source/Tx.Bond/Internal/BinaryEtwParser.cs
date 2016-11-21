// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    using Tx.Windows;

    internal sealed class BinaryEtwParser
    {
        /// <summary>
        /// The Provider used to log Binary objects in ETW stream.
        /// </summary>
        internal static readonly Guid EtwBinaryEventManifestProviderId = BinaryEventSource.Log.Guid;


        private readonly List<byte[]> eventCache = new List<byte[]>();
        private uint currentEventPackageId;

        private readonly List<string> manifestCache = new List<string>();
        private uint currentManifestPackageId;

        private readonly Guid etwProviderId;

        public BinaryEtwParser(Guid etwProviderId)
        {
            this.etwProviderId = etwProviderId;
        }

        internal Envelope Parse(EtwNativeEvent etwNativeEvent)
        {
            Envelope result = null;

            if (etwNativeEvent.ProviderId == this.etwProviderId)
            {
                switch (etwNativeEvent.Id)
                {
                    case 0:
                        result = ParseV0(etwNativeEvent);
                        break;
                    case 1:
                        result = ParseV1(etwNativeEvent);
                        break;
                    case 2:
                        result = this.ParseV2(etwNativeEvent);
                        break;
                }
            }

            return result;
        }

        internal EventManifest ParseManifest(EtwNativeEvent etwNativeEvent)
        {
            EventManifest result = null;

            if (etwNativeEvent.ProviderId == this.etwProviderId)
            {
                switch (etwNativeEvent.Id)
                {
                    case 3:
                        result = ParseRegularManifest(etwNativeEvent);
                        break;
                    case 4:
                        result = this.ParseChunkedManifest(etwNativeEvent);
                        break;
                }
            }

            return result;
        }

        private static Envelope ParseV0(EtwNativeEvent etwNativeEvent)
        {
            // Reading like this ensures that if exception is thrown, we know what failed
            long occurenceFileTimeUtc = etwNativeEvent.ReadInt64();
            long receiveFileTimeUtc = etwNativeEvent.ReadInt64();
            string protocol = etwNativeEvent.ReadUnicodeString();

            bool isValid = !(protocol ?? string.Empty).StartsWith(@"rceException: ", StringComparison.OrdinalIgnoreCase);

            string source;
            string manifestId;
            uint eventPayloadLength;
            byte[] eventPayload;

            if (isValid)
            {
                source = etwNativeEvent.ReadUnicodeString();
                manifestId = etwNativeEvent.ReadUnicodeString();
                eventPayloadLength = etwNativeEvent.ReadUInt32(); // There is a side-effect being used here with the binary length.                
                // Payload overflow events also could be saved with event Id 0
                eventPayload = (eventPayloadLength < 65000) ? etwNativeEvent.ReadBytes() : new byte[0];
            }
            else
            {
                protocol = string.Empty;
                source = string.Empty;
                manifestId = string.Empty;
                eventPayloadLength = 0;
                eventPayload = new byte[0];
            }

            return new Envelope(
                occurenceFileTimeUtc >= 0
                    ? DateTimeOffset.FromFileTime(occurenceFileTimeUtc)
                    : DateTimeOffset.MinValue,
                receiveFileTimeUtc >= 0
                    ? DateTimeOffset.FromFileTime(receiveFileTimeUtc)
                    : DateTimeOffset.MinValue,
                protocol,
                source,
                manifestId,
                eventPayload,
                null);
        }

        private static EventManifest ParseRegularManifest(EtwNativeEvent etwNativeEvent)
        {
            // EventId is one. This is a log written using EventSource Byte Array logging support.
            // Reading like this ensures that if exception is thrown, we know what failed
            long occurenceFileTimeUtc = etwNativeEvent.ReadInt64();
            long receiveFileTimeUtc = etwNativeEvent.ReadInt64();
            string protocol = etwNativeEvent.ReadUnicodeString();
            string source = etwNativeEvent.ReadUnicodeString();
            string manifestId = etwNativeEvent.ReadUnicodeString();
            string manifest = etwNativeEvent.ReadUnicodeString();

            return new EventManifest
            {
                ActivityId = etwNativeEvent.ActivityId,
                OccurenceFileTimeUtc = occurenceFileTimeUtc,
                ReceiveFileTimeUtc = receiveFileTimeUtc,
                Protocol = protocol,
                Source = source,
                ManifestId = manifestId,
                Manifest = manifest,
            };
        }

        private EventManifest ParseChunkedManifest(EtwNativeEvent etwNativeEvent)
        {
            uint packageId = etwNativeEvent.ReadUInt32();
            long occurenceFileTimeUtc = etwNativeEvent.ReadInt64();
            long receiveFileTimeUtc = etwNativeEvent.ReadInt64();
            string protocol = etwNativeEvent.ReadUnicodeString();
            string source = etwNativeEvent.ReadUnicodeString();
            string manifestId = etwNativeEvent.ReadUnicodeString();
            uint chunkCount = etwNativeEvent.ReadUInt32();
            uint currentChunkNumber = etwNativeEvent.ReadUInt32();
            string manifest = etwNativeEvent.ReadUnicodeString();

            if (chunkCount == 1)
            {
                this.manifestCache.Clear();

                return new EventManifest
                {
                    ActivityId = etwNativeEvent.ActivityId,
                    OccurenceFileTimeUtc = occurenceFileTimeUtc,
                    ReceiveFileTimeUtc = receiveFileTimeUtc,
                    Protocol = protocol,
                    Source = source,
                    ManifestId = manifestId,
                    Manifest = manifest
                };
            }
            else if (chunkCount > currentChunkNumber)
            {
                if (this.currentManifestPackageId != packageId || this.manifestCache.Count != currentChunkNumber)
                {
                    this.manifestCache.Clear();
                    this.currentManifestPackageId = packageId;
                }

                this.manifestCache.Add(manifest);

                if (chunkCount == (currentChunkNumber + 1))
                {
                    string payload = string.Join("", this.manifestCache.ToArray());
                    this.manifestCache.Clear();

                    return new EventManifest
                    {
                        ActivityId = etwNativeEvent.ActivityId,
                        OccurenceFileTimeUtc = occurenceFileTimeUtc,
                        ReceiveFileTimeUtc = receiveFileTimeUtc,
                        Protocol = protocol,
                        Source = source,
                        ManifestId = manifestId,
                        Manifest = payload,
                    };
                }
            }
            else
            {
                this.manifestCache.Clear();
            }

            return null;
        }

        private static Envelope ParseV1(EtwNativeEvent etwNativeEvent)
        {
            // EventId is one. This is a log written using EventSource Byte Array logging support.
            // Reading like this ensures that if exception is thrown, we know what failed
            long occurenceFileTimeUtc = etwNativeEvent.ReadInt64();
            long receiveFileTimeUtc = etwNativeEvent.ReadInt64();
            string protocol = etwNativeEvent.ReadUnicodeString();
            string source = etwNativeEvent.ReadUnicodeString();
            string manifestId = etwNativeEvent.ReadUnicodeString();
            uint eventPayloadLength = etwNativeEvent.ReadUInt32(); // There is a side-effect being used here with the binary length.
            etwNativeEvent.ReadInt32(); // EventSource based byte array writer actually stores the byte array length here. Skip 4 bytes to account for it.
            byte[] eventPayload = etwNativeEvent.ReadBytes();

            return new Envelope(
                occurenceFileTimeUtc >= 0
                    ? DateTimeOffset.FromFileTime(occurenceFileTimeUtc)
                    : DateTimeOffset.MinValue,
                receiveFileTimeUtc >= 0
                    ? DateTimeOffset.FromFileTime(receiveFileTimeUtc)
                    : DateTimeOffset.MinValue,
                protocol,
                source,
                manifestId,
                eventPayload,
                null);
        }

        private Envelope ParseV2(EtwNativeEvent etwNativeEvent)
        {
            uint packageId = etwNativeEvent.ReadUInt32();
            long occurenceFileTimeUtc = etwNativeEvent.ReadInt64();
            long receiveFileTimeUtc = etwNativeEvent.ReadInt64();
            string protocol = etwNativeEvent.ReadUnicodeString();
            string source = etwNativeEvent.ReadUnicodeString();
            string manifestId = etwNativeEvent.ReadUnicodeString();
            uint chunkCount = etwNativeEvent.ReadUInt32();
            uint currentChunkNumber = etwNativeEvent.ReadUInt32();
            uint eventPayloadLength = etwNativeEvent.ReadUInt32(); // There is a side-effect being used here with the binary length.
            etwNativeEvent.ReadUInt32();
            byte[] eventPayload = etwNativeEvent.ReadBytes();

            if (chunkCount == 1)
            {
                this.eventCache.Clear();

                return new Envelope(
                    occurenceFileTimeUtc >= 0
                        ? DateTimeOffset.FromFileTime(occurenceFileTimeUtc)
                        : DateTimeOffset.MinValue,
                    receiveFileTimeUtc >= 0
                        ? DateTimeOffset.FromFileTime(receiveFileTimeUtc)
                        : DateTimeOffset.MinValue,
                    protocol,
                    source,
                    manifestId,
                    eventPayload,
                    null);
            }
            else if (chunkCount > currentChunkNumber)
            {
                if (this.currentEventPackageId != packageId || this.eventCache.Count != currentChunkNumber)
                {
                    this.eventCache.Clear();
                    this.currentEventPackageId = packageId;
                }

                this.eventCache.Add(eventPayload);

                if (chunkCount == (currentChunkNumber + 1))
                {
                    var payload = ByteArrayHelper.Join(this.eventCache);
                    this.eventCache.Clear();

                    return new Envelope(
                        occurenceFileTimeUtc >= 0
                            ? DateTimeOffset.FromFileTime(occurenceFileTimeUtc)
                            : DateTimeOffset.MinValue,
                        receiveFileTimeUtc >= 0
                            ? DateTimeOffset.FromFileTime(receiveFileTimeUtc)
                            : DateTimeOffset.MinValue,
                        protocol,
                        source,
                        manifestId,
                        eventPayload,
                        null);
                }
            }
            else
            {
                this.eventCache.Clear();
            }

            return null;
        }
    }
}
