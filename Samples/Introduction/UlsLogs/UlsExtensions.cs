using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

namespace UlsLogs
{
    public static class UlsExtensions
    {
        public static void AddUlsFiles(this IPlaybackConfiguration playback, params string[] ulsFiles)
        {
            playback.AddInput(
               () => UlsObservable.FromFiles(ulsFiles),
               typeof(UlsTypeMap));
        }
    }
}
