using Bond;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Tx.Binary;

namespace BondInEtwLinqpadDriver
{
    public class TypeCache
    {
        public string CacheDirectory { get; private set; }
        public EventManifest[] Manifests { get; private set; }

        private readonly string gbcPath;

        public TypeCache()
        {
            gbcPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"gbc.exe");
        }

        public static string Resolve(string targetDir)
        {
            return Path.Combine(ResolveCacheDirectory(targetDir), "BondTypes.dll");
        }

        public static string ResolveCacheDirectory(string targetDir)
        {
            var path = Path.Combine(Path.GetTempPath(), "BondEtwV2Cache", targetDir);

            return path;
        }

        public void Init(string targetDir, string[] files)
        {
            CacheDirectory = ResolveCacheDirectory(targetDir);

            if (!Directory.Exists(CacheDirectory)) // Todo: do all below once, if we have to create the directory
                Directory.CreateDirectory(CacheDirectory);

            Manifests = BinaryEtwObservable.BinaryManifestFromSequentialFiles(files)
                .ToEnumerable()
                .GroupBy(manifest => manifest.ManifestId, StringComparer.OrdinalIgnoreCase)
                .Select(grp => grp.First())
                .ToArray();
            if (Manifests.Length == 0)
                throw new Exception("No Bond manifests found");

            var sources = new List<string>(Manifests.Length);

            foreach (var m in Manifests)
            {
                string bondFileName = Path.Combine(CacheDirectory, "BondTypes" + sources.Count + ".bond");

                File.WriteAllText(bondFileName, m.Manifest);

                var source = GenerateCSharpCode(bondFileName);

                sources.Add(source);
            }

            string asm = Path.Combine(CacheDirectory, "BondTypes.dll");
            OutputAssembly(sources.ToArray(), asm);
        }

        private string GenerateCSharpCode(string bondFileName)
        {
            var command = "c# " + "\"" + bondFileName + "\"" + " -o=" + "\"" + CacheDirectory;

            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = gbcPath,
                Arguments = command,
                WorkingDirectory = CacheDirectory,
                RedirectStandardOutput = true
            };

            using (var gbc = Process.Start(processStartInfo))
            {
                var reader = gbc.StandardOutput;
                gbc.WaitForExit();
                var output = reader.ReadToEnd();
                var output2 = output;
            }

            var flename = Path.GetFileNameWithoutExtension(bondFileName) + "_types.cs";
            string fullFileName = Path.Combine(CacheDirectory, flename);
            string source = File.ReadAllText(fullFileName);

            return source;
        }

        private string[] GetAssemblies(string targetDir)
        {
            return Directory.GetFiles(CacheDirectory, @"*.dll");
        }

        public Type[] Types
        {
            get
            {
                Type[] types = new Type[0];

                try
                {
                    types = GetAssemblies(CacheDirectory)
                        .Select(Assembly.LoadFrom)
                        .SelectMany(assembly => assembly.GetTypes().Where(type => type.IsPublic))
                        .ToArray();
                }
                catch (Exception)
                {
                    // Ignore
                }

                return types;
            }
        }

        public static Type[] GetTypes(string targetDir)
        {
            Type[] types = new Type[0];

            try
            {
                types = Assembly.LoadFrom(Resolve(targetDir))
                    .GetTypes()
                    .Where(type => type.IsPublic)
                    .ToArray();
            }
            catch (Exception)
            {
                // Ignore
            }

            return types;            
        }

        private static void OutputAssembly(string[] sources, string assemblyPath)
        {
            var providerOptions = new Dictionary<string, string> {{"CompilerVersion", "v4.0"}};

            string[] assemblyNames = 
                {    
                    // Todo: how to remove the  hack below?
                    @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.dll",
                    //typeof (Attribute).Assembly.Location, // ?
                    typeof (ObservableCollection<>).Assembly.Location, // System
                    typeof (Expression).Assembly.Location, // System.Core
                    typeof (BondDataType).Assembly.Location, // Bond
                    typeof (RequiredAttribute).Assembly.Location // Bond.Attributes
                };

            using (var codeProvider = new CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new CompilerParameters(
                    assemblyNames,
                    assemblyPath, 
                    false);

                CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sources);

                if (results.Errors.Count == 0)
                    return;

                var sb = new StringBuilder();
                foreach (object o in results.Errors)
                {
                    sb.AppendLine(o.ToString());
                }

                string errors = sb.ToString();
                throw new Exception(errors);
            }
        }
    }
}