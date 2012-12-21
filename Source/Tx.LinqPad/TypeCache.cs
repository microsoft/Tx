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

                DateTime manifestTimestamp = File.GetLastWriteTimeUtc(f);

                if (File.Exists(output))
                {
                    DateTime outputTimestamp = File.GetLastWriteTimeUtc(output);

                    if (outputTimestamp == manifestTimestamp)
                        continue;
                }

                string manifest = File.ReadAllText(f);
                Dictionary<string, string> generated = ManifestParser.Parse(manifest);

                AssemblyBuilder.OutputAssembly(generated, output);
                File.SetLastWriteTimeUtc(output, manifestTimestamp);
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
