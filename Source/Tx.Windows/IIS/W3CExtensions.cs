// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public static class W3CExtensions
    {
        //[FileParser("IIS W3C logs", ".log")]
        public static void AddW3CLogFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => W3CEnumerable.FromFiles(files).ToObservable(ThreadPoolScheduler.Instance),
                typeof(W3CTypeMap));
        }
    }
}
