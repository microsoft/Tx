// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using global::Bond;
    using global::Bond.IO.Safe;
    using global::Bond.Protocols;

    using Tx.Binary;

    /// <summary>
    /// The load bond type.
    /// </summary>
    public class BondTypeMap : BinaryTypeMap<BinaryEnvelope, string>
    {
        public BondTypeMap()
            : this(GetExecutingAssemblyLocation())
        {
        }

        public BondTypeMap(
            string lookupFolder)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            if (lookupFolder == null)
            {
                throw new ArgumentNullException("lookupFolder");
            }

            if (!Directory.Exists(lookupFolder))
            {
                throw new ArgumentException("Specified folder does not exist.", "lookupFolder");
            }

            this.Initialize(lookupFolder);
        }

        /// <summary>
        /// Gets the time function.
        /// </summary>
        public override Func<BinaryEnvelope, DateTimeOffset> TimeFunction
        {
            get
            {
                return GetTime;                 
            }
        }

        public override string GetInputKey(BinaryEnvelope envelope)
        {
            return envelope.PayloadId;
        }

        private static DateTimeOffset GetTime(BinaryEnvelope envelope)
        {
            var time = DateTime.FromFileTimeUtc(envelope.ReceiveFileTimeUtc);

            return time;
        }

        private void Initialize(string lookupFolder)
        {
            var bondTypes = EnumerateBondTypes(lookupFolder);

            foreach (var currentType in bondTypes)
            {
                try
                {
                    this.RegisterBondType(currentType);
                }
                catch (ReflectionTypeLoadException exception)
                {
                    BinaryEventSource.Log.Error("Error trying to load type, " + currentType.AssemblyQualifiedName + ", error: " + exception);

                    if (exception.LoaderExceptions != null)
                    {
                        foreach (var loaderException in exception.LoaderExceptions)
                        {
                            if (loaderException != null)
                            {
                                BinaryEventSource.Log.Error(loaderException.ToString());
                            }
                        }
                    }

                    throw;
                }
                catch (FileLoadException exception)
                {
                    BinaryEventSource.Log.Error("Error trying to load type, " + currentType.AssemblyQualifiedName + ", error: " + exception);
                    throw;
                }
            }
        }

        private static string GetExecutingAssemblyLocation()
        {
            string currentFolder;

            try
            {
                var location = Assembly.GetExecutingAssembly().Location;
                currentFolder = Path.GetDirectoryName(location);
            }
            catch (Exception exception)
            {
                BinaryEventSource.Log.Error("Error trying to get executing assembly location, error: " + exception);
                currentFolder = ".";
            }

            return currentFolder;
        }

        private static IEnumerable<Type> EnumerateBondTypes(params string[] folders)
        {
            var types = folders
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(GetAllAssembliesInFolder)
                .SelectMany(a => a.GetTypes())
                .Where(TypeExtensions.IsBondType)
                .ToArray();

            return types;
        }

        private void RegisterBondType(Type type)
        {
            var manifestId = type.GetBondManifestIdentifier();

            var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(type);

            this.PayloadConverterCache.Add(
                type,
                new KeyValuePair<string, Func<BinaryEnvelope, object>>(
                    manifestId,
                    e => GetBondObject(e, deserializer)));
        }
        
        private static object GetBondObject(BinaryEnvelope envelope, Deserializer<CompactBinaryReader<InputBuffer>> deserializer)
        {
            var inputStream = new InputBuffer(envelope.EventPayload);

            var version = string.Equals(envelope.Protocol, BondProtocol.CompactBinaryV1, StringComparison.Ordinal)
                ? (ushort)1
                : (ushort)2;

            var reader = new CompactBinaryReader<InputBuffer>(inputStream, version);

            object outputObject;

            try
            {
                outputObject = deserializer.Deserialize(reader);
            }
            catch (Exception exception)
            {
                outputObject = null;
                BinaryEventSource.Log.Error("Error trying to deserialize payload for " + envelope.PayloadId + ", error: " + exception);
            }

            return outputObject;
        }

        private static List<Assembly> GetAllAssembliesInFolder(string folder)
        {
            var assemblyFiles = Directory.EnumerateFiles(
                folder, 
                "*.*", 
                SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || 
                     f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

            var assemblies = new List<Assembly>();

            foreach (var dll in assemblyFiles)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(dll));
                }
                catch (BadImageFormatException)
                {
                    // e.g. x64 assemblies.
                }
            }

            return assemblies;
        }
    }
}
