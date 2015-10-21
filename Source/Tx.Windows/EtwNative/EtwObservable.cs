// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading;

namespace Tx.Windows
{
    /// <summary>
    ///     Factory for creating raw ETW (Event Tracing for Windows) observable sources
    /// </summary>
    public static class EtwObservable
    {
        /// <summary>
        ///     Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="etlFiles">up to 63 files to read</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<EtwNativeEvent> FromFiles(params string[] etlFiles)
        {
            if (etlFiles == null)
                throw new ArgumentNullException("etlFiles");

            if (etlFiles.Length == 0 || etlFiles.Length > 63)
                throw new ArgumentException("the supported count of files is from 1 to 63");

            return new NonDetachObservable<EtwNativeEvent>(o => new EtwFileReader(o, etlFiles));
        }

        /// <summary>
        ///     Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="startTime">start time of sequence of events</param>
        /// <param name="endTime">end time of sequence of events</param>
        /// <param name="etlFiles">up to 63 files to read</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<EtwNativeEvent> FromFiles(DateTime startTime, DateTime endTime, params string[] etlFiles)
        {
            if (etlFiles == null)
                throw new ArgumentNullException("etlFiles");

            if (etlFiles.Length == 0 || etlFiles.Length > 63)
                throw new ArgumentException("the supported count of files is from 1 to 63");

            return new NonDetachObservable<EtwNativeEvent>(o => new EtwFileReader(o, false, startTime, endTime, etlFiles));
        }

        /// <summary>
        ///     Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="etlFiles">Unlimited number of ETL files containing events ordered by timestamp</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<EtwNativeEvent> FromSequentialFiles(params string[] etlFiles)
        {
            if (etlFiles == null)
                throw new ArgumentNullException("etlFiles");

            if (etlFiles.Length == 0)
                throw new ArgumentException("the supported count of files is atleast one file");

            return new NonDetachObservable<EtwNativeEvent>(o => new EtwFileReader(o, true, etlFiles));
        }

        /// <summary>
        ///     Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="startTime">start time of sequence of events</param>
        /// <param name="endTime">end time of sequence of events</param>
        /// <param name="etlFiles">Unlimited number of ETL files containing events ordered by timestamp</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<EtwNativeEvent> FromSequentialFiles(DateTime startTime, DateTime endTime, params string[] etlFiles)
        {
            if (etlFiles == null)
                throw new ArgumentNullException("etlFiles");

            if (etlFiles.Length == 0)
                throw new ArgumentException("the supported count of files is atleast one file");

            return new NonDetachObservable<EtwNativeEvent>(o => new EtwFileReader(o, true, startTime, endTime, etlFiles));
        }

        /// <summary>
        ///     Creates a listener to ETW real-time session
        /// </summary>
        /// <param name="sessionName">session name</param>
        /// <returns>events received from the session</returns>
        public static IObservable<EtwNativeEvent> FromSession(string sessionName)
        {
            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            return new NonDetachObservable<EtwNativeEvent>(o => new EtwListener(o, sessionName));
        }

        /// <summary>
        ///     Extracts manifest from .etl file that was produced using System.Diagnostics.Tracing.EventSource
        /// </summary>
        /// <param name="etlFile">Trace file</param>
        /// <returns></returns>
        public static string[] ExtractManifests(string etlFile)
        {
            IObservable<EtwNativeEvent> all = FromFiles(etlFile);

            var manifests = new Dictionary<Guid, ManifestReassembly>();
            var evt = new ManualResetEvent(false);

            IDisposable d = all.Subscribe(e =>
                {
                    if (e.Id != 0xfffe) // 65534
                    {
                        return;
                    }

                    byte format = e.ReadByte();
                    if (format != 1)
                        throw new Exception("Unsuported manifest format found in EventSource event" + format);

                    byte majorVersion = e.ReadByte();
                    byte minorVersion = e.ReadByte();
                    byte magic = e.ReadByte();
                    if (magic != 0x5b)
                        throw new Exception("Unexpected content in EventSource event that was supposed to have manifest");

                    ushort totalChunks = e.ReadUInt16();
                    ushort chunkNumber = e.ReadUInt16();

                    string chunk = e.ReadAnsiString();
                    ManifestReassembly ra = null;
                    if (!manifests.TryGetValue(e.ProviderId, out ra))
                    {
                        ra = new ManifestReassembly
                        {
                            Totalchunks = totalChunks,
                            LastChunkNumber = chunkNumber,
                            Manifest = new StringBuilder(chunk)
                        };

                        manifests.Add(e.ProviderId, ra);
                        return;
                    }

                    if (chunkNumber <= ra.LastChunkNumber)
                        return;
                    else if (chunkNumber == ra.LastChunkNumber + 1)
                    {
                        ra.LastChunkNumber = chunkNumber;
                        ra.Manifest.Append(chunk);
                    }
                    else 
                        throw new Exception("Missing chunks when trying to concatenate the manifest for provider " + e.ProviderId);

                },
                () => evt.Set());

            evt.WaitOne();
            d.Dispose();

            var result = new List<string>();

            foreach (ManifestReassembly ra in manifests.Values)
                result.Add(ra.Manifest.ToString());
            var r = result.ToArray();
            return r;
        }

        class ManifestReassembly
        {
            public int Totalchunks { get; set; }
            public int LastChunkNumber { get; set; }
            public StringBuilder Manifest { get; set; }
        }
     }
}