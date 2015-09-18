// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Reactive;
    using Tx.Binary;

    public static class EtwExtensions
    {
        [FileParser("Bond Event Trace Log", ".etl")]
        public static void AddBondEtlFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (files == null)
            {
                throw new ArgumentNullException("files");
            }

            playback.AddInput(() => BinaryEtwObservable.FromFiles(files), typeof(GeneralPartitionableTypeMap));
        }

        [FileParser("Sequential Bond Event Trace Log", ".etl")]
        public static void AddSequentialBondEtlFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (files == null)
            {
                throw new ArgumentNullException("files");
            }

            playback.AddInput(() => BinaryEtwObservable.FromSequentialFiles(files), typeof(GeneralPartitionableTypeMap));
        }
    }
}
