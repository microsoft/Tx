// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive;

namespace Tx.Windows
{
    public static class EtwExtensions
    {
        [RealTimeFeed("ETW", "Event Tracing for Windows")]
        public static void AddRealTimeSession(this IPlaybackConfiguration playback, string session)
        {
            playback.AddInput(
                () => EtwObservable.FromSession(session),
                typeof (EtwManifestTypeMap),
                typeof (EtwClassicTypeMap),
                typeof (EtwTypeMap));
        }

        [FileParser("Event Trace Log", ".etl")]
        public static void AddEtlFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => EtwObservable.FromFiles(files),
                typeof (EtwManifestTypeMap),
                typeof (EtwClassicTypeMap),
                typeof (EtwTypeMap));
        }

        [FileParser("Sequential Event Trace Logs", ".etl")]
        public static void AddEtlFileSequence(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => EtwObservable.FromSequentialFiles(files),
                typeof(EtwManifestTypeMap),
                typeof(EtwClassicTypeMap),
                typeof(EtwTypeMap));
        }
    }
}