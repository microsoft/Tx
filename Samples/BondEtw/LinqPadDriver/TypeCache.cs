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
    using System.Globalization;

    public class TypeCache
    {
        private readonly string _gbcPath;

        private readonly string[] _assemblyNames = 
        {    
            // Todo: how to remove the  hack below?
            @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.dll",
            //typeof (Attribute).Assembly.Location, // ?
            typeof (ObservableCollection<>).Assembly.Location, // System
            typeof (Expression).Assembly.Location, // System.Core
            typeof (BondDataType).Assembly.Location, // Bond
            typeof (RequiredAttribute).Assembly.Location // Bond.Attributes
        };

        private const string _partialClassTemplate = @"namespace {0}
{{
    using System.Runtime.InteropServices;

    [Guid(""{1}"")]
    public partial class {2}
    {{
    }}
}}
";

        public string CacheDirectory { get; private set; }
        public EventManifest[] Manifests { get; private set; }

        public TypeCache()
        {
            this._gbcPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"gbc.exe");
        }

        public static string Resolve(string targetDir)
        {
            return Path.Combine(ResolveCacheDirectory(targetDir), @"BondTypes.dll");
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

            foreach (var m in Manifests)
            {
                var source = GenerateManifestOverrideCSharpClass(
                    m.Manifest,
                    m.ManifestId);

                sources.Add(source);
            }

            string asm = Path.Combine(CacheDirectory, @"BondTypes.dll");
            OutputAssembly(sources.ToArray(), asm);
        }

        private string GenerateManifestOverrideCSharpClass(string manifest, string manifestId)
        {
            var lines = manifest.Split('\n');

            var line = lines
                .FirstOrDefault(l => l.Trim().StartsWith(@"namespace ", StringComparison.OrdinalIgnoreCase));

            var @namespace = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            line = lines
                .LastOrDefault(l => l.Trim().StartsWith(@"struct ", StringComparison.OrdinalIgnoreCase));

            var className = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            var source = string.Format(
                CultureInfo.InvariantCulture, 
                _partialClassTemplate,
                @namespace, 
                manifestId,
                className);

            return source;
        }

        private string GenerateCSharpCode(string bondFileName)
        {
            var command = "c# " + "\"" + bondFileName + "\"" + " -o=" + "\"" + CacheDirectory;

            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = this._gbcPath,
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

        private void OutputAssembly(string[] sources, string assemblyPath)
        {
            var providerOptions = new Dictionary<string, string> {{"CompilerVersion", "v4.0"}};

            using (var codeProvider = new CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new CompilerParameters(
                    this._assemblyNames,
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