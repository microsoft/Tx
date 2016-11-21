// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;

    internal static class ByteArrayHelper
    {
        internal static byte[] Join(IList<byte[]> chunks)
        {
            int size = 0;

            checked
            {
                for (var i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    if (chunk != null)
                    {
                        size += chunk.Length;
                    }
                }
            }

            var result = new byte[size];

            var seek = 0;
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkLength = chunk.Length;

                Array.Copy(chunk, 0, result, seek, chunkLength);
                seek += chunkLength;
            }

            return result;
        }

        internal static byte[][] Split(this byte[] source, int limit)
        {
            if (source.Length <= limit)
            {
                throw new ArgumentOutOfRangeException("limit");
            }

            var chunkCount = ((source.Length - 1) / limit) + 1;
            var chunks = new byte[chunkCount][];

            var lastChunkIndex = chunkCount - 1;
            var currentPosition = 0;
            for (int i = 0; i < lastChunkIndex; i++)
            {
                var chunk = new byte[limit];
                Array.Copy(source, currentPosition, chunk, 0, limit);
                chunks[i] = chunk;
                currentPosition += limit;
            }

            var lastChunkLength = Math.Min(limit, source.Length - currentPosition);
            var lastChunk = new byte[lastChunkLength];
            Array.Copy(source, currentPosition, lastChunk, 0, lastChunkLength);
            chunks[lastChunkIndex] = lastChunk;

            return chunks;
        }
    }
}
