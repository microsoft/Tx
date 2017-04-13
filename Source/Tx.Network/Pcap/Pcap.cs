// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

// The Pcap format is: https://wiki.wireshark.org/Development/LibpcapFileFormat
// The C# implementation below reads files in .pcap format

namespace Tx.Network
{
    public class Pcap
    {
        /// <summary>
        /// Reads a file and returns the parsed <see cref="PcapRecord"/>s.
        /// </summary>
        /// <param name="filename">The name of the file to parse.</param>
        /// <returns>The parsed <see cref="PcapRecord"/>s.</returns>
        public static IEnumerable<PcapRecord> ReadFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                foreach (var r in ReadStream(stream))
                    yield return r;
            }
        }

        /// <summary>
        /// Reads a <see cref="Stream"/> and returns the parsed <see cref="PcapRecord"/>s.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to parse.</param>
        /// <returns>The parsed <see cref="PcapRecord"/>s.</returns>
        public static IEnumerable<PcapRecord> ReadStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new BinaryReader(stream))
            {
                int pos = 0;
                int length = (int)reader.BaseStream.Length;
                if (length <= (24 + 16))
                {
                    yield break;
                }

                var magicNumber = reader.ReadUInt32();
                var version_major = reader.ReadUInt16();
                var version_minor = reader.ReadUInt16();
                var thiszone = reader.ReadInt32();
                var sigfigs = reader.ReadUInt32();
                var snaplen = reader.ReadUInt32();
                var network = reader.ReadUInt32();

                pos += 24;

                while ((pos + 16) < length)
                {
                    var ts_sec = reader.ReadUInt32();
                    var ts_usec = reader.ReadUInt32();
                    var incl_len = reader.ReadUInt32();
                    var orig_len = reader.ReadUInt32();

                    pos += 16;

                    if ((pos + incl_len) > length)
                    {
                        yield break;
                    }

                    var data = reader.ReadBytes((int)incl_len);
                    pos += (int)incl_len;

                    yield return new PcapRecord
                    {
                        Data = data,
                        NetworkId = network,
                        Timestamp = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)
                            .AddSeconds(ts_sec + thiszone)
                            .AddMilliseconds((ts_usec == 0 || ts_usec >= 1000000) ? ts_usec : ts_usec / 1000)
                    };
                }
            }
        }
    }

    public class PcapRecord
    {
        public DateTimeOffset Timestamp;
        public uint NetworkId;
        public byte[] Data;
    }
}
