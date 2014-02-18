// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public static class PlaybackConfigurationExtensions
    {
        /// <summary>
        ///     Method for adding input sequence that contains .NET objects to the playbackConfiguration
        /// </summary>
        /// <param name="playbackConfiguration">The playback instance.</param>
        /// <param name="source">The input sequence.</param>
        public static void AddInput(this IPlaybackConfiguration playbackConfiguration, IEnumerable<Timestamped<object>> source)
        {
            if (playbackConfiguration == null)
            {
                throw new ArgumentNullException("playbackConfiguration");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            playbackConfiguration
                .AddInput(
                    () => source.ToObservable(Scheduler.Default),
                    typeof(PartitionableTypeMap));
        }

        private sealed class PartitionableTypeMap : IPartitionableTypeMap<Timestamped<object>, string>
        {
            public Func<Timestamped<object>, object> GetTransform(Type outputType)
            {
                return item => item.Value;
            }

            public Func<Timestamped<object>, DateTimeOffset> TimeFunction
            {
                get
                {
                    return item => item.Timestamp;
                }
            }

            public string GetTypeKey(Type outputType)
            {
                return outputType.FullName;
            }

            public string GetInputKey(Timestamped<object> evt)
            {
                return evt.Value.GetType().FullName;
            }

            public IEqualityComparer<string> Comparer
            {
                get
                {
                    return StringComparer.Ordinal;
                }
            }
        }
    }
}
