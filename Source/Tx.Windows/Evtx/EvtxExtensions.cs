using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public static class EvtxExtensions
    {
        [FileParser(".evtx", "Event Log")]
        public static void AddLogFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => EvtxEnumerable.FromFiles(files).ToObservable(ThreadPoolScheduler.Instance),
                typeof(EvtxManifestTypeMap),
                typeof(EvtxTypeMap));
        }
    }
}
