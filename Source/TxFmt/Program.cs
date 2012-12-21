using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reflection;
using Tx.Windows;

namespace TxFmt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(
@"Usage: TxFmt files...
 
Supported files are 
    .man   : Manifest
    .etl   : Event Trace Log
    .evtx  : Event Log");

                Environment.Exit(1);
            }

            try
            {
                Playback pb = new Playback();

                string asmDir = Path.Combine(Path.GetTempPath(), "TxFmt");
                if (Directory.Exists(asmDir))
                    Directory.Delete(asmDir, true);
                Directory.CreateDirectory(asmDir);

                foreach (string a in args)
                {
                    string ext = Path.GetExtension(a).ToLower();
                    switch (ext)
                    {
                        case ".etl":
                            pb.AddEtlFiles(a);
                            break;

                        case ".evtx":
                            pb.AddLogFiles(a);
                            break;

                        case ".man":
                            string manifest = File.ReadAllText(a);
                            var generated = ManifestParser.Parse(manifest);

                            string assemblyPath = Path.Combine(asmDir, Path.ChangeExtension(Path.GetFileName(a),".dll"));
                            AssemblyBuilder.OutputAssembly(generated, assemblyPath);
                            break;

                        default:
                            throw new Exception("unknown extension " + ext);
                    }
                }

                List<Type> knownTypes = new List<Type>();

                foreach (string a in Directory.GetFiles(asmDir, "*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(a);
                    knownTypes.AddRange(assembly.GetTypes());
                }

                pb.KnownTypes = knownTypes.ToArray();

                IObservable<SystemEvent> all = pb.GetObservable<SystemEvent>();
                all.Subscribe(Console.WriteLine);
                pb.Run();
            }
            catch (Exception ex)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
                Console.ForegroundColor = color;
            }
        }
    }
}
