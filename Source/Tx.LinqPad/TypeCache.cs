// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Tx.Windows;
using System.Reflection;

namespace Tx.LinqPad
{
    class TypeCache
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

                Dictionary<string, string> sources;
                switch(Path.GetExtension(f).ToLower())
                {
                    case ".man":
                        {
                            string manifest = File.ReadAllText(f);
                            sources = ManifestParser.Parse(manifest);
                            break;
                        }

                    case ".etl":
                        {
                            string manifest = ManifestParser.ExtractFromTrace(f);
                            if (manifest == "")
                                continue;

                            sources = ManifestParser.Parse(manifest);
                            break;
                        }

                    case ".blg":
                    case ".csv":
                    case ".tsv":
                        sources = PerfCounterParser.Parse(f);
                        break;

                    default:
                        throw new Exception("Unknown metadata format " + f);
                }

                AssemblyBuilder.OutputAssembly(sources, output);
                File.SetLastWriteTimeUtc(output, metadataTimestamp);
            }
        }

        public Assembly[] GetAssemblies(string targetDir, string[] traces, string[] metadaFiles)
        {

            Assembly[] assemblies = (from file in Directory.GetFiles(GetCacheDir(targetDir), "*.dll")
                    select Assembly.LoadFrom(file)).ToArray();

            return assemblies;
        }

        public Type[] GetAvailableTypes(string targetDir, string[] traces, string[] metadaFiles)
        {
            Assembly[] assemblies = GetAssemblies(targetDir, traces, metadaFiles);

            return (from a in assemblies
                    from t in a.GetTypes()
                    where t.IsPublic
                    select t).ToArray();
        }

        string GetCacheDir(string targetDir)
        {
            return Path.Combine(Path.GetTempPath(), "TxTypeCache", targetDir); 
        }
    }
}
