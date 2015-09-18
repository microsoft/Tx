// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.Script.Serialization;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;

    public sealed class BondEtwObserver : IObserver<object>, IDisposable
    {
        private readonly IDictionary<Type, string> manifestMap;

        private readonly OutputBuffer outputBuffer = new OutputBuffer();

        private readonly CompactBinaryWriter<OutputBuffer> writer;

        private readonly JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

        private TimeSpan interval = TimeSpan.FromMinutes(20);

        private Timer logManifestTimer;

        private IDictionary<Type, BondTypeInfo> bondTypeMap;

        private ConcurrentBag<BondTypeInfo> knownManifest;

        /// <summary>
        /// Initializes a new instance of the <see cref="BondEtwObserver"/> class.
        /// </summary>
        public BondEtwObserver()
        {
            this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BondEtwObserver"/> class.
        /// </summary>
        /// <param name="manifestMap">The map of manifests per types.</param>
        /// <param name="interval">The time period between writing manifests into ETW.</param>
        /// <exception cref="System.ArgumentNullException">manifestMap</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">interval</exception>
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

            this.writer = new CompactBinaryWriter<OutputBuffer>(this.outputBuffer);

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

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
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
                string manifest;

                if (type.IsBondStruct())
                {
                    manifest = type.TryGetManifestData();

                    manifestData = new BondTypeInfo
                    {
                        ManifestId = type.GetBondManifestIdentifier(),
                        ManifestData = manifest,
                        Serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(type)
                    };

                    this.bondTypeMap.Add(type, manifestData);
                }
                else
                {
                    manifest = null;

                    manifestData = new BondTypeInfo
                    {
                        ManifestId = type.GetBondManifestIdentifier(),
                        ManifestData = null,
                        Serializer = null
                    };

                    this.bondTypeMap.Add(type, manifestData);
                }

                if (manifest != null)
                {
                    this.knownManifest.Add(manifestData);

                    if (this.logManifestTimer == null)
                    {
                        this.InitializeTimer();
                    }
                }
            }

            var now = DateTime.UtcNow;

            if (manifestData.Serializer != null)
            {
                this.outputBuffer.Position = 0;

                manifestData.Serializer.Serialize(value, this.writer);

                BinaryEventSource.Log.Write(now, now, BondProtocol.CompactBinaryV1, @"Tx.Bond", this.outputBuffer.Data.ToByteArray(), manifestData.ManifestId);
            }
            else
            {
                var json = this.javaScriptSerializer.Serialize(value);
                BinaryEventSource.Log.Write(
                    now,
                    now,
                    "JSON",
                    @"Tx.Bond",
                    Encoding.UTF8.GetBytes(json),
                    manifestData.ManifestId);
            }
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            BinaryEventSource.Log.Error(error.ToString());
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
            catch(Exception error)
            {
                BinaryEventSource.Log.Error(error.ToString());
            }
            finally
            {
                if (this.logManifestTimer != null)
                {
                    this.logManifestTimer.Change((int)this.interval.TotalMilliseconds, Timeout.Infinite);
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
