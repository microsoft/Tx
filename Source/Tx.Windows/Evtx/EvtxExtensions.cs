// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public static class EvtxExtensions
    {
        [FileParser("Event Logs", ".evtx")]
        public static void AddLogFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => EvtxEnumerable.FromFiles(files).ToObservable(ThreadPoolScheduler.Instance),
                typeof(EvtxManifestTypeMap),
                typeof(EvtxTypeMap));
        }
    }
}
