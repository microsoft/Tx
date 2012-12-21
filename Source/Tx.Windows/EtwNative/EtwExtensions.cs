using System.Reactive;

namespace Tx.Windows
{
    public static class EtwExtensions
    {
        [RealTimeFeed("ETW","Event Tracing for Windows")]
        public static void AddRealTimeSession(this IPlaybackConfiguration playback, string session)
        {
            playback.AddInput(
                () => EtwObservable.FromSession(session),
                typeof(EtwManifestTypeMap),
                typeof(EtwClassicTypeMap),
                typeof(EtwTypeMap));
        }

        [FileParser(".etl", "Event Trace Log")]
        public static void AddEtlFiles(this IPlaybackConfiguration playback, params string[] files)
        {
            playback.AddInput(
                () => EtwObservable.FromFiles(files),
                typeof(EtwManifestTypeMap),
                typeof(EtwClassicTypeMap),
                typeof(EtwTypeMap));
        }
    }
}
