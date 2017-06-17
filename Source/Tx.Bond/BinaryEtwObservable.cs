// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;

    using Tx.Windows;

    /// <summary>
    /// Uses Tx.Windows implementation of Etw log parser and converts the objects into BinaryEnvelope objects. Use a BinaryConfigTypeMap derived implementation
    /// along with this class to convert into required datatypes.
    /// </summary>
    public static class BinaryEtwObservable
    {
        /// <summary>
        /// Takes an array of files and creates an observer that retrieves all BinaryEnvelope type events. These Events have EventId 0 
        /// and belong to EtwBinaryEventManifestProviderId
        /// </summary>
        /// <param name="files">Up to 63 files to read.</param>
        /// <returns>Sequence of events ordered by timestamp.</returns>
        public static IObservable<IEnvelope> FromFiles(params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, false, null, null, files);
        }

        /// <summary>
        /// Takes an array of files and creates an observer that retrieves all BinaryEnvelope type events. These Events have EventId 0 
        /// and belong to EtwBinaryEventManifestProviderId
        /// </summary>
        /// <param name="startTime">Start time of sequence of events, if null then DateTime.MinValue will be used.</param>
        /// <param name="endTime">End time of sequence of events, if null then DateTime.MaxValue will be used.</param>
        /// <param name="files">Up to 63 files to read.</param>
        /// <returns>Sequence of events ordered by timestamp.</returns>
        public static IObservable<IEnvelope> FromFiles(DateTime startTime, DateTime endTime, params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, false, startTime, endTime, files);
        }

        public static IObservable<IEnvelope> FromSequentialFiles(params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, true, null, null, files);
        }

        public static IObservable<IEnvelope> FromSequentialFiles(DateTime startTime, DateTime endTime, params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, true, startTime, endTime, files);
        }

        public static IObservable<EventManifest> BinaryMainfestFromFiles(params string[] files)
        {
            return BinaryManifestFromFiles(BinaryEventSource.Log.Guid, false, null, null, files);
        }

        public static IObservable<EventManifest> BinaryMainfestFromFiles(DateTime startTime, DateTime endTime, params string[] files)
        {
            return BinaryManifestFromFiles(BinaryEventSource.Log.Guid, false, startTime, endTime, files);
        }

        public static IObservable<EventManifest> BinaryManifestFromSequentialFiles(params string[] files)
        {
            return BinaryManifestFromFiles(BinaryEventSource.Log.Guid, true, null, null, files);
        }

        public static IObservable<EventManifest> BinaryManifestFromSequentialFiles(DateTime startTime, DateTime endTime, params string[] files)
        {
            return BinaryManifestFromFiles(BinaryEventSource.Log.Guid, true, startTime, endTime, files);
        }

        /// <summary>
        /// Takes an array of files and creates an observer that retrieves all BinaryEnvelope type events. These Events have EventId 0, 1 and 2.
        /// and belong to specified provider.
        /// </summary>
        /// <param name="providerId">Identifier of ETW provider.</param>
        /// <param name="useSequentialReader">Flag to specify if the input ETL files are already ordered by timestamp.</param>
        /// <param name="startTime">Start time of sequence of events, if null then DateTime.MinValue will be used.</param>
        /// <param name="endTime">End time of sequence of events, if null then DateTime.MaxValue will be used.</param>
        /// <param name="files">Either unlimited number of ETL files containing events ordered by timestamp or up to 63 files to read.</param>
        /// <returns>Sequence of events ordered by timestamp.</returns>
        public static IObservable<IEnvelope> FromFiles(
            Guid providerId,
            bool useSequentialReader,
            DateTime? startTime,
            DateTime? endTime,
            params string[] files)
        {
            var parser = new BinaryEtwParser(providerId);

            var etwObservable = CreateEtwObservable(useSequentialReader, startTime, endTime, files);

            return etwObservable
                .Select(parser.Parse)
                .Where(item => item != null);
        }

        /// <summary>
        /// Creates a listener to ETW real-time session for BinaryEnvelope events. These Events have EventId 0, 1 and 2.
        /// and belong to specified provider.
        /// </summary>
        /// <param name="providerId">Identifier of ETW provider.</param>
        /// <param name="sessionName">Session name.</param>
        /// <returns>Sequence of events ordered by timestamp.</returns>
        public static IObservable<IEnvelope> FromSession(
            Guid providerId,
            string sessionName)
        {
            var parser = new BinaryEtwParser(providerId);

            var etwObservable = EtwObservable.FromSession(sessionName);

            return etwObservable
                .Select(parser.Parse)
                .Where(item => item != null);
        }

        public static IObservable<EventManifest> BinaryManifestFromFiles(
            Guid providerId,
            bool useSequentialReader,
            DateTime? startTime, 
            DateTime? endTime, 
            params string[] files)
        {
            var parser = new BinaryEtwParser(providerId);

            var etwObservable = CreateEtwObservable(useSequentialReader, startTime, endTime, files);

            return etwObservable
                .Select(parser.ParseManifest)
                .Where(item => item != null);
        }

        private static IObservable<EtwNativeEvent> CreateEtwObservable(
            bool useSequentialReader,
            DateTime? startTime,
            DateTime? endTime,
            params string[] files)
        {
            if (startTime.HasValue != endTime.HasValue)
            {
                throw new ArgumentException("Specify both start and end times or leave both of them null.");
            }

            if (startTime.HasValue && startTime.Value >= endTime.Value)
            {
                throw new ArgumentException("Start time should be less than end time.");
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (files.Length == 0)
            {
                throw new ArgumentException("The Files parameter should contain at least one element.");
            }

            IObservable<EtwNativeEvent> etwObservable;

            if (useSequentialReader)
            {
                if (startTime.HasValue)
                {
                    etwObservable = EtwObservable.FromSequentialFiles(startTime.Value, endTime.Value, files);
                }
                else
                {
                    etwObservable = EtwObservable.FromSequentialFiles(files);
                }
            }
            else
            {
                if (startTime.HasValue)
                {
                    etwObservable = EtwObservable.FromFiles(startTime.Value, endTime.Value, files);
                }
                else
                {
                    etwObservable = EtwObservable.FromFiles(files);
                }
            }

            return etwObservable;
        }
    }
}
