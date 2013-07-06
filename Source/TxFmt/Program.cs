// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reflection;
using Tx.Windows;

namespace TxFmt
{
    internal class Program
    {
        private static void Main(string[] args)
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
                var pb = new Playback();

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
                            Dictionary<string, string> generated = ManifestParser.Parse(manifest);

                            string assemblyPath = Path.Combine(asmDir, Path.ChangeExtension(Path.GetFileName(a), ".dll"));
                            AssemblyBuilder.OutputAssembly(generated, new string[]{}, assemblyPath);
                            break;

                        default:
                            throw new Exception("unknown extension " + ext);
                    }
                }

                var knownTypes = new List<Type>();

                foreach (string a in Directory.GetFiles(asmDir, "*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(a);
                    knownTypes.AddRange(assembly.GetTypes());
                }

                pb.KnownTypes = knownTypes.ToArray();

                IObservable<SystemEvent> all = pb.GetObservable<SystemEvent>();
                all.Subscribe(e=>
                    {
                        if (!e.ToString().StartsWith(" DocumentServiceId"))
                        {
                            Console.WriteLine("{0} {1}", e.Header.EventId, e.ToString());
                        };
                    });
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