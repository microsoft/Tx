// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;

    public sealed class BondEtwObserver : IObserver<object>, IDisposable
    {
        private IDictionary<Type, string> manifestMap;

        private TimeSpan interval = TimeSpan.FromMinutes(20);

        private Timer logManifestTimer;

        private IDictionary<Type, BondTypeInfo> bondTypeMap;

        private ConcurrentBag<BondTypeInfo> knownManifest;

        private readonly OutputBuffer outputBuffer = new OutputBuffer();

        private readonly CompactBinaryWriter<OutputBuffer> writer;

        public BondEtwObserver()
        {
            this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
        }

        public BondEtwObserver(
            IDictionary<Type, string> manifestMap, 
            TimeSpan interval)
        {
            if (manifestMap == null)
            {
                throw new ArgumentNullException("manifestMap");
            }

            if (interval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval");
            }

            this.manifestMap = manifestMap;
            this.interval = interval;

            // V2. must be used explicitly.
            this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer, 2);

            this.Initialize();
        }
        
        public void Initialize()
        {
            if (this.bondTypeMap == null)
            {
                this.bondTypeMap = (this.manifestMap ?? new Dictionary<Type, string>())
                    .ToDictionary(
                        m => m.Key,
                        m => new BondTypeInfo
                        {
                            ManifestId = m.Key.GetBondManifestIdentifier(),
                            ManifestData = m.Value,
                            Serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(m.Key)
                        });

                this.knownManifest = new ConcurrentBag<BondTypeInfo>(this.bondTypeMap
                    .Select(i => i.Value)
                    .Where(i => i != null));

                if (this.knownManifest.Count > 0)
                {
                    this.InitializeTimer();
                }
            }
        }

        public void OnNext(object value)
        {
            if (value == null)
            {
                return;
            }

            this.Initialize();

            var type = value.GetType();

            BondTypeInfo manifestData;
            if (!this.bondTypeMap.TryGetValue(type, out manifestData))
            {
                if (!type.IsBondType())
                {
                    throw new NotSupportedException();
                }

                var manifest = type.TryGetManifestData();

                manifestData = new BondTypeInfo
                {
                    ManifestId = type.GetBondManifestIdentifier(),
                    ManifestData = manifest,
                    Serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(type)
                };

                this.bondTypeMap.Add(type, manifestData);

                if (manifest != null)
                {
                    this.knownManifest.Add(manifestData);

                    if (this.logManifestTimer == null)
                    {
                        this.InitializeTimer();
                    }
                }
            }

            this.outputBuffer.Position = 0;

            manifestData.Serializer.Serialize(value, this.writer);

            var now = DateTime.UtcNow;
            BinaryEventSource.Log.Write(now, now, BondProtocol.CompactBinaryV1, "Tx.Bond", this.outputBuffer.Data.ToArray(), manifestData.ManifestId);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        private void InitializeTimer()
        {
            this.logManifestTimer = new Timer(this.LogManifest, null, Timeout.Infinite, Timeout.Infinite);
            this.logManifestTimer.Change(0, Timeout.Infinite);
        }

        private void LogManifest(object status)
        {
            try
            {
                var currentTime = DateTime.UtcNow;

                foreach (var manfiestInfo in this.knownManifest)
                {
                    BinaryEventSource.Log.WriteManifest(
                        currentTime,
                        currentTime,
                        "BondManifest",
                        "BondEtwObserver",
                        manfiestInfo.ManifestId,
                        manfiestInfo.ManifestData);
                }
            }
            finally
            {
                if (this.logManifestTimer != null)
                {
                    this.logManifestTimer.Change(this.interval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        public void Dispose()
        {
            if (this.logManifestTimer != null)
            {
                this.logManifestTimer.Dispose();
            }
        }

        internal sealed class BondTypeInfo
        {
            public string ManifestId { get; set; }

            public Serializer<CompactBinaryWriter<OutputBuffer>> Serializer { get; set; }

            public string ManifestData { get; set; }
        }
    }
}
