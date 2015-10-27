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
        public void Init(string targetDir, string[] files)
        {
            CacheDirectory = Path.Combine(Path.GetTempPath(), "BondEtwV2Cache", targetDir);

            if (!Directory.Exists(CacheDirectory)) // Todo: do all below once, if we have to create the directory
                Directory.CreateDirectory(CacheDirectory);

                Manifests = BinaryEtwObservable.BinaryManifestFromSequentialFiles(files)
                        .ToEnumerable()
                        .GroupBy(manifest => manifest.ManifestId, StringComparer.OrdinalIgnoreCase)
                        .Select(grp => grp.First())
                        .ToArray();
                if (Manifests.Length == 0)
                    throw new Exception("No Bond manifests found");

                StringBuilder sb = new StringBuilder();
                foreach (var m in Manifests)
                {
                    sb.AppendLine(m.Manifest);
                    sb.AppendLine();
                }
            
                string gbcPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "gbc.exe");

                string bondFileName = Path.Combine(CacheDirectory, "BondTypes.bond");
                File.WriteAllText(bondFileName, sb.ToString());

                var command = "c# " + "\"" + bondFileName + "\"" + " -o=" + "\"" + CacheDirectory;

                var processStartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = gbcPath,
                    Arguments = command,
                    WorkingDirectory = CacheDirectory
                };

                Process gbc = Process.Start(processStartInfo);
                gbc.WaitForExit();

                string csFileName = Path.Combine(CacheDirectory, "BondTypes_types.cs");
                string source = File.ReadAllText(csFileName);

                string asm = Path.Combine(CacheDirectory, "BondTypes.dll");
                OutputAssembly(new string[] { source }, asm);
        }

        public string[] GetAssemblies(string targetDir)
        {
            return Directory.GetFiles(CacheDirectory, "*.dll");
        }

        public Type[] Types
        {
            get
            {
                Assembly[] assemblies = (from file in GetAssemblies(CacheDirectory)
                                         select Assembly.LoadFrom(file)).ToArray();

                var types = (from a in assemblies
                             from t in a.GetTypes()
                             where t.IsPublic
                             select t).ToArray();

                return types;
            }
        }

        public static void OutputAssembly(string[] sources, string assemblyPath)
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