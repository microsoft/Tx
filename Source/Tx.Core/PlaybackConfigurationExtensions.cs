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
                throw new ArgumentNullException(nameof(playbackConfiguration));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            playbackConfiguration
                .AddInput(
                    () => source.ToObservable(Scheduler.Default),
                    typeof(PartitionableTypeMap<object>));
        }

        /// <summary>
        ///     Method for adding input sequence from CSV files.
        /// </summary>
        /// <param name="playback">The playback instance.</param>
        /// <param name="transformation">Transformation function converting Record to T</param>
        /// <param name="timestampSelector">Timestamp selector function.</param>
        /// <param name="files">CSV files containing events ordered by timestamp.</param>
        [FileParser("Default CSV parser", ".csv")]
        public static void AddCsvInput<T>(
            this IPlaybackConfiguration playback,
            Func<Record, T> transformation,
            Func<Record, DateTimeOffset> timestampSelector,
            params string[] files)
        {
            if (playback == null)
            {
                throw new ArgumentNullException(nameof(playback));
            }

            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            if (timestampSelector == null)
            {
                throw new ArgumentNullException(nameof(timestampSelector));
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            AddCsvInput(playback, ',', 0, transformation, timestampSelector, files);
        }

        /// <summary>
        ///     Method for adding input sequence from custom CSV files.
        /// </summary>
        /// <param name="playback">The playback instance.</param>
        /// <param name="delimiter"></param>
        /// <param name="numberRecordsToSkip"></param>
        /// <param name="transformation">Transformation function converting Record to T</param>
        /// <param name="timestampSelector">Timestamp selector function.</param>
        /// <param name="files">CSV files containing events ordered by timestamp.</param>
        [FileParser("Custom CSV parser", ".csv", ".tsv", ".txt")]
        public static void AddCsvInput<T>(
            this IPlaybackConfiguration playback,
            char delimiter,
            int numberRecordsToSkip,
            Func<Record, T> transformation,
            Func<Record, DateTimeOffset> timestampSelector,
            params string[] files)
        {
            if (playback == null)
            {
                throw new ArgumentNullException(nameof(playback));
            }

            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            if (timestampSelector == null)
            {
                throw new ArgumentNullException(nameof(timestampSelector));
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            playback.AddInput(
                () => new CsvObservable(delimiter, numberRecordsToSkip)
                    .FromFiles(files)
                    .Select(item => new Timestamped<T>(transformation(item), timestampSelector(item))),
                typeof(PartitionableTypeMap<T>));
        }

        public static IObservable<TOutput> OfType<TInput, TOutput>(
            this IObservable<TInput> source,
            params ITypeMap<TInput>[] typeMaps)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (typeMaps == null)
            {
                throw new ArgumentNullException(nameof(typeMaps));
            }

            return Observable
                .Create<Timestamped<object>>(observer =>
                {
                    var deserialzier = new CompositeDeserializer<TInput>(observer, typeMaps);
                    deserialzier.EndTime = DateTime.MaxValue;
                    deserialzier.AddKnownType(typeof(TOutput));
                    return source.SubscribeSafe(deserialzier);
                })
                .Select(i => i.Value)
                .Where(i => i != null)
                .OfType<TOutput>();
        }

        private sealed class PartitionableTypeMap<T> : IPartitionableTypeMap<Timestamped<T>, string>
        {
            public Func<Timestamped<T>, object> GetTransform(Type outputType)
            {
                return item => item.Value;
            }

            public Func<Timestamped<T>, DateTimeOffset> TimeFunction
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

            public string GetInputKey(Timestamped<T> evt)
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
