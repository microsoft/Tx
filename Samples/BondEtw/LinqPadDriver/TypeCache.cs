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

namespace Tx.Bond.LinqPad
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
            // Don't validate targetDir for empty. it will be created.
            if (targetDir == null)
            {
                throw new ArgumentNullException("targetDir");
            }

            if (files == null || files.Length <= 0)
            {
                throw new ArgumentNullException("files");
            }

            Stopwatch sw = Stopwatch.StartNew();
            CacheDirectory = ResolveCacheDirectory(targetDir);

            if (!Directory.Exists(CacheDirectory))
            {
                // Todo: do all below once, if we have to create the directory
                Directory.CreateDirectory(CacheDirectory);
            }
            else
            {
                // Clean
                Directory.Delete(CacheDirectory, true);

                // Recreate.
                Directory.CreateDirectory(CacheDirectory);
            }

            var manifestsFromFiles = BinaryEtwObservable.BinaryManifestFromSequentialFiles(files)
                .ToEnumerable()
                .GroupBy(manifest => manifest.ManifestId, StringComparer.OrdinalIgnoreCase)
                .Select(grp => grp.First())
                .ToArray();

            if (manifestsFromFiles.Length == 0)
            {
                throw new Exception("No Bond manifests found");
            }

            var sources = new List<string>();

            List<EventManifest> manifests = new List<EventManifest>();

            foreach (var manifestTypes in manifestsFromFiles
                .GroupBy(i => i.Manifest, StringComparer.Ordinal))
            {
                string bondFileName = Path.Combine(CacheDirectory, "BondTypes" + sources.Count + ".bond");

                File.WriteAllText(bondFileName, manifestTypes.Key);

                var source = this.GenerateCSharpCode(bondFileName);

                if (!string.IsNullOrWhiteSpace(source))
                {
                    manifests.Add(manifestTypes.First());
                    sources.Add(source);
                }

                var manifestInfo = this.ParseClassNames(manifestTypes.Key);

                sources.AddRange(GenerateAdditionalSourceCodeItems(manifestInfo, manifestTypes.Select(i => i.ManifestId).ToArray()));
            }

            this.Manifests = manifests.ToArray();

            string asm = Path.Combine(CacheDirectory, @"BondTypes.dll");
            OutputAssembly(sources.ToArray(), asm);

            sw.Stop();

            Console.WriteLine("TypeCache Init took {0} milliseconds.", sw.ElapsedMilliseconds);
        }

        public IEnumerable<string> GenerateAdditionalSourceCodeItems(
            Tuple<string, string[]> bondManifest,
            string[] manifestIds)
        {
            if (bondManifest.Item1 == null)
            {
                return new string[0];
            }

            if (manifestIds.Length == 1)
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var className in bondManifest.Item2)
                {
                    var name = bondManifest.Item1 + "." + className;

                    var id = BondIdentifierHelpers.GenerateGuidFromName(name);

                    map[name] = id.ToString();
                }

                if (!string.Equals(
                    manifestIds[0],
                    map[bondManifest.Item1 + "." + bondManifest.Item2.Last()],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return new string[]
                    {
                        GenerateManifestOverrideCSharpClass(bondManifest.Item1, bondManifest.Item2.Last(), manifestIds[0]),
                    };
                }
            }

            return new string[0];
        }

        public Tuple<string, string[]> ParseClassNames(string manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest))
            {
                throw new ArgumentNullException("manifest");
            }

            var lines = manifest.Split('\n');

            var line = lines
                .FirstOrDefault(l => l.Trim().StartsWith(@"namespace ", StringComparison.OrdinalIgnoreCase));

            var @namespace = (line ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            var classNamesObtained = lines
                .Where(l => l.Trim().StartsWith(@"struct ", StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Split(new[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            
            var classNamesValid = classNamesObtained.Where(a => CodeCompiler.IsValidLanguageIndependentIdentifier(a)).ToArray();

            return new Tuple<string, string[]>(
                @namespace,
                classNamesValid);
        }

        private string GenerateManifestOverrideCSharpClass(string @namespace, string className, string manifestId)
        {
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
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            string error = string.Empty;
            using (var gbc = Process.Start(processStartInfo))
            {
                var reader = gbc.StandardOutput;
                gbc.WaitForExit();
                var output = reader.ReadToEnd();
                var output2 = output;

                error = gbc.StandardError.ReadToEnd();
            }

            string source = null;
            if (string.IsNullOrWhiteSpace(error))
            {
                var filename = Path.GetFileNameWithoutExtension(bondFileName) + "_types.cs";
                string fullFileName = Path.Combine(CacheDirectory, filename);
                source = File.ReadAllText(fullFileName);
            }

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