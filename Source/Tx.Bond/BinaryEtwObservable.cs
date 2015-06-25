// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;
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
        /// <param name="files"></param>
        /// <returns></returns>
        public static IObservable<BinaryEnvelope> FromFiles(params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, false, null, null, files);
        }

        public static IObservable<BinaryEnvelope> FromFiles(DateTime startTime, DateTime endTime, params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, false, startTime, endTime, files);
        }

        public static IObservable<BinaryEnvelope> FromSequentialFiles(params string[] files)
        {
            return FromFiles(BinaryEventSource.Log.Guid, true, null, null, files);
        }

        public static IObservable<BinaryEnvelope> FromSequentialFiles(DateTime startTime, DateTime endTime, params string[] files)
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

        public static IObservable<BinaryEnvelope> FromFiles(
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
                throw new ArgumentNullException("files");
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
