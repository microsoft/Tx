// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.SqlServer.XEvent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using Tx.SqlServer;
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
            List<string> extraAssembies = new List<string>();

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
                    case ".manifest":
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
                                Dictionary<string, string> s;                                
                                try
                                {
                                    s = ManifestParser.Parse(manifest);
                                }
                                catch (XmlException)
                                {
                                    // if one manifest is bad, we should still see the other events
                                    string err = String.Format(
                                        "Malformed manifest found in the file {0}\nThe corresponding events will not be shown in the tree-control. \nHere are the first 1000 characters: \n\n{1}", f, manifest.Substring(0,1000));
                                    MessageBox.Show(err, "Tx LINQPad Driver");
                                    continue; 
                                }
 

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

                    case ".xel":
                        {
                            extraAssembies.Add(typeof(XEventAttribute).Assembly.Location);
                            extraAssembies.Add(typeof(CallStack).Assembly.Location);

                            Dictionary<string, string> s = XeTypeGenerator.Parse(f);
                            foreach (string type in s.Keys)
                            {
                                if (!sources.ContainsKey(type))
                                {
                                    sources.Add(type, s[type]);
                                }
                            }
                        }
                        break;

                    default:
                        throw new Exception("Unknown metadata format " + f);
                }

                AssemblyBuilder.OutputAssembly(sources, extraAssembies, output);
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

            var types = (from a in assemblies
                         from t in a.GetTypes()
                         where t.IsPublic
                         select t).ToArray();

            return types;
        }

        private string GetCacheDir(string targetDir)
        {
            return Path.Combine(Path.GetTempPath(), "TxTypeCache", targetDir);
        }
    }
}