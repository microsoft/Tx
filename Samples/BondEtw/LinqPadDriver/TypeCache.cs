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
using Tx.Bond;

namespace Tx.Bond.LinqPad
{
    using BondEtwDriver;
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

        public IList<TypeCacheItem> Cache
        {
            get;
            private set;
        }

        public TypeCache()
        {
            this._gbcPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"gbc.exe");

            this.Cache = new List<TypeCacheItem>();
        }

        public static string Resolve(string targetDir)
        {
            return Path.Combine(ResolveCacheDirectory(targetDir), "*.dll");
        }

        public static string ResolveCacheDirectory(string targetDir)
        {
            var path = Path.Combine(Path.GetTempPath(), "BondEtwV2Cache", targetDir);

            return path;
        }

        public void Initialize(string targetDir, params string[] files)
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

            this.CacheDirectory = ResolveCacheDirectory(targetDir);

            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            else
            {
                Utilities.EmptyDirectory(this.CacheDirectory);
            }

            // Get all manifests.
            var manifestsFromFiles = BinaryEtwObservable.BinaryManifestFromSequentialFiles(files)
                .ToEnumerable()
                .Where(a => !string.IsNullOrWhiteSpace(a.Manifest))
                .GroupBy(manifest => manifest.ManifestId, StringComparer.OrdinalIgnoreCase)
                .Select(grp => grp.First())
                .ToArray();

            if (manifestsFromFiles.Length == 0)
            {
                throw new Exception("No Bond manifests found. Ensure that the Bond manifests are written to the ETL file.");
            }

            int counter = 0;
            foreach (var manifestItem in manifestsFromFiles)
            {
                var codeSources = new List<string>();

                var namespaceAndClasses = this.ParseClassNames(manifestItem.Manifest);

                var assembliesOfThisManifest = new List<string>();

                if (!this.AreAnyTypesAlreadyInCache(namespaceAndClasses.Item2))
                {
                    string bondFileName = Path.Combine(CacheDirectory, "BondTypes" + counter + ".bond");

                    counter++;

                    File.WriteAllText(bondFileName, manifestItem.Manifest);
                    var codeGenerated = this.GenerateCSharpCode(bondFileName);

                    if (!string.IsNullOrWhiteSpace(codeGenerated))
                    {
                        codeSources.Add(codeGenerated);

                        foreach (var @class in namespaceAndClasses.Item2)
                        {
                            var completeName = namespaceAndClasses.Item1 + "." + @class;

                            var id = BondIdentifierHelpers.GenerateGuidFromName(@class.ToUpperInvariant());

                            // case when single manifest has multiple structs.
                            if (namespaceAndClasses.Item2.Length > 1 && 
                                !string.Equals(manifestItem.ManifestId, id.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                codeSources.Add(GenerateAdditionalSourceCodeItems(namespaceAndClasses.Item1, @class, id));

                                this.Cache.Add(new TypeCacheItem
                                    {
                                        Manifest = new EventManifest
                                        {
                                            ActivityId = manifestItem.ActivityId,
                                            Manifest = manifestItem.Manifest,
                                            ManifestId = id.ToString(),
                                            OccurenceFileTimeUtc = manifestItem.OccurenceFileTimeUtc,
                                            Protocol = manifestItem.Protocol,
                                            ReceiveFileTimeUtc = manifestItem.ReceiveFileTimeUtc,
                                            Source = manifestItem.Source,
                                        },
                                    });
                            }
                            else
                            {
                                codeSources.Add(GenerateAdditionalSourceCodeItems(namespaceAndClasses.Item1, @class, new Guid(manifestItem.ManifestId)));
                                this.Cache.Add(new TypeCacheItem
                                {
                                    Manifest = new EventManifest
                                    {
                                        ActivityId = manifestItem.ActivityId,
                                        Manifest = manifestItem.Manifest,
                                        ManifestId = manifestItem.ManifestId,
                                        OccurenceFileTimeUtc = manifestItem.OccurenceFileTimeUtc,
                                        Protocol = manifestItem.Protocol,
                                        ReceiveFileTimeUtc = manifestItem.ReceiveFileTimeUtc,
                                        Source = manifestItem.Source,
                                    },
                                });
                            }
                        }

                        // After building the assembly, the types will be available.
                        string asm = Path.Combine(CacheDirectory, bondFileName + ".dll");
                        this.OutputAssembly(codeSources.ToArray(), asm);
                        assembliesOfThisManifest.Add(asm);

                        try
                        {
                            var types = assembliesOfThisManifest.Select(Assembly.LoadFile)
                                        .SelectMany(a => a.GetTypes().Where(type => type.IsPublic))
                                        .ToArray();

                            foreach (var item in types)
                            {
                                var targetCacheItem = this.FindMatchOrDefault(item.GUID.ToString());


                                if (targetCacheItem != null)
                                {
                                    targetCacheItem.Type = item;
                                }
                            }
                        }
                        catch
                        {
                            //Ignore this type
                            throw;
                        }
                    }
                    
                }
            }

        }

        public string GenerateAdditionalSourceCodeItems(string @namespace, string @class, Guid manifestId)
        {
            if (string.IsNullOrWhiteSpace(@namespace) || string.IsNullOrWhiteSpace(@class) || manifestId == Guid.Empty)
            {
                return string.Empty;
            }

            var code = GenerateManifestOverrideCSharpClass(@namespace, @class, manifestId.ToString());

            return code;
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
                .Select(l => l.Split(new[] { ' ', '{', ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            
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

        public static Type[] GetTypes(string targetDir)
        {
            Type[] types = new Type[0];

            try
            {
                var assemblies = Directory.GetFiles(ResolveCacheDirectory(targetDir), "*.dll");

                types = assemblies.Select(Assembly.LoadFrom)
                    .SelectMany(a => a.GetTypes().Where(type => type.IsPublic))
                    .ToArray();
            }
            catch
            {
                // Ignore
            }

            return types;            
        }

        private void OutputAssembly(string[] sources, string assemblyPath)
        {
            Utilities.ForceDeleteFile(assemblyPath);

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

        private bool AreAnyTypesAlreadyInCache(string[] typeNames)
        {
            bool result = false;
            foreach (var cacheItem in this.Cache)
            {
                if (result)
                {
                    break;
                }

                foreach (var item in typeNames)
                {
                    if (cacheItem.Type != null && string.Equals(cacheItem.Type.Name, item, StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public TypeCacheItem FindMatchOrDefault(string manifestId)
        {
            foreach (var item in this.Cache)
            {
                if (string.Equals(item.Manifest.ManifestId, manifestId, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }


            return null;
        }
    }
}