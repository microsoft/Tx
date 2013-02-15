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
        public static void Init()
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
        }

        public static void BuildCache(string manifestDir)
        {
            string[] files = Directory.GetFiles(manifestDir, "*.man");
            BuildCache(files);
        }

        public static void BuildCache(string[] metadaFiles)
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);

            foreach (string f in metadaFiles)
            {
                string output = Path.Combine(CacheDir, 
                        Path.ChangeExtension(
                            Path.GetFileName(f), 
                            ".dll"));

                DateTime metadataTimestamp = File.GetLastWriteTimeUtc(f);

                Dictionary<string, string> sources;
                switch(Path.GetExtension(f).ToLower())
                {
                    case ".man":
                        string manifest = File.ReadAllText(f);
                        sources = ManifestParser.Parse(manifest);
                        break;

                    case ".blg":
                        sources = PerfCounterParser.Parse(f);
                        break;

                    default:
                        throw new Exception("Unknown metadata format " + f);
                }

                AssemblyBuilder.OutputAssembly(sources, output);
            }
        }

        public static Assembly[] Assemblies
        {
            get 
            {
                Assembly[] assemblies = (from file in Directory.GetFiles(CacheDir, "*.dll")
                        select Assembly.LoadFrom(file)).ToArray();

                return assemblies;
            }
        }
            
        public static Type[] AvailableTypes
        {
            get 
            {
                Assembly[] assemblies = (from file in Directory.GetFiles(CacheDir, "*.dll")
                                         select Assembly.LoadFrom(file)).ToArray();

                return (from a in assemblies
                        from t in a.GetTypes()
                        where t.IsPublic
                        select t).ToArray();
            }
        }

        static string CacheDir
        {
            get { return Path.Combine(Path.GetTempPath(), "TxTypeCache"); }
        }
    }
}
