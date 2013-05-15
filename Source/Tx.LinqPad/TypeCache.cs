// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tx.Windows;

namespace Tx.LinqPad
{
    internal class TypeCache
    {
        public void Init(string targetDir)
        {
            if (!Directory.Exists(GetCacheDir(targetDir)))
                Directory.CreateDirectory(GetCacheDir(targetDir));
        }

        public void BuildCache(string targetDir, string[] traces, string metadataDir)
        {
            string[] metadaFiles = Directory.GetFiles(metadataDir, "*.man");
            BuildCache(targetDir, traces, metadaFiles);
        }

        public void BuildCache(string targetDir, string[] traces, string[] metadaFiles)
        {
            foreach (string f in metadaFiles.Concat(traces))
            {
                string output = Path.Combine(GetCacheDir(targetDir),
                                             Path.ChangeExtension(
                                                 Path.GetFileName(f),
                                                 ".dll"));

                DateTime metadataTimestamp = File.GetLastWriteTimeUtc(f);
                DateTime outputTimestamp = File.GetLastWriteTimeUtc(output);

                if (outputTimestamp == metadataTimestamp)
                    continue;

                var sources = new Dictionary<string, string>();
                switch (Path.GetExtension(f).ToLower())
                {
                    case ".man":
                        {
                            string manifest = File.ReadAllText(f);
                            Dictionary<string, string> s = ManifestParser.Parse(manifest);
                            foreach (string type in s.Keys)
                            {
                                if (!sources.ContainsKey(type))
                                {
                                    sources.Add(type, s[type]);
                                }
                            }
                            break;
                        }

                    case ".etl":
                        {
                            string[] manifests = ManifestParser.ExtractFromTrace(f);
                            if (manifests.Length == 0)
                                continue;

                            foreach (string manifest in manifests)
                            {
                                Dictionary<string, string> s = ManifestParser.Parse(manifest);

                                foreach (string type in s.Keys)
                                {
                                    if (!sources.ContainsKey(type))
                                    {
                                        sources.Add(type, s[type]);
                                    }
                                }
                            }
                        }
                        break;

                    case ".blg":
                    case ".csv":
                    case ".tsv":
                        {
                            Dictionary<string, string> s = PerfCounterParser.Parse(f);
                            foreach (string type in s.Keys)
                            {
                                if (!sources.ContainsKey(type))
                                {
                                    sources.Add(type, s[type]);
                                }
                            }
                        }
                        break;

                    case ".evtx":
                        break;

                    default:
                        throw new Exception("Unknown metadata format " + f);
                }

                AssemblyBuilder.OutputAssembly(sources, output);
                File.SetLastWriteTimeUtc(output, metadataTimestamp);
            }
        }

        public string[] GetAssemblies(string targetDir, string[] traces, string[] metadaFiles)
        {
            return Directory.GetFiles(GetCacheDir(targetDir), "*.dll");
        }

        public Type[] GetAvailableTypes(string targetDir, string[] traces, string[] metadaFiles)
        {
            Assembly[] assemblies = (from file in GetAssemblies(targetDir, traces, metadaFiles)
                                     select Assembly.LoadFrom(file)).ToArray();

            return (from a in assemblies
                    from t in a.GetTypes()
                    where t.IsPublic
                    select t).ToArray();
        }

        private string GetCacheDir(string targetDir)
        {
            return Path.Combine(Path.GetTempPath(), "TxTypeCache", targetDir);
        }
    }
}