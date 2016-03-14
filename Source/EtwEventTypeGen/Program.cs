// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Tx.Windows;

namespace Microsoft.Etw
{
    internal class Program
    {
        private static string assembly = "";
        private static string outputDirectory = "";
        private static readonly Dictionary<string, string> generated = new Dictionary<string, string>();

        private static void Main(string[] args)
        {
            Parse(args);

            if (!String.IsNullOrEmpty(outputDirectory))
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }

            if (String.IsNullOrEmpty(assembly))
            {
                OutputCode();
            }
            else
            {
                string target = Path.Combine(outputDirectory, assembly);
                AssemblyBuilder.OutputAssembly(generated, new string[] { }, target);
            }
        }

        private static void Parse(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    @"Usage: 
    EtwEventTypeGen [/o:dir] [/a:name] [/m:file] [/e:file] [/t:file] [/w:path]

Switches:
    /o:dir       Directory for the output. 
                 If missing, the output is written to the current directory

    /a:name      Generate Assembly. 
                 If missing the output is C# files.

    /m:manifest	 Input from manifest(s)
    /e:etl       Input from eventsource etl(s) that contain manifests
    /t:tmf       Input from TMF file(s)
    /p:file.blg  Input from performance counter trace

The switches /o and /a must occur at most once.

The switches /m /t can occur many times. 
At least one of these switches must be present

Examples:

    EtwEventTypeGen /m:Microsoft-Windows-HttpService.man
    EtwEventTypeGen /o:c:\Code\EventTypes /m:*.man
    EtwEventTypeGen /a:MyEventTypes /t:c:\tmfs\*.tmf /m:*.man
    EtwEventTypeGen /a:WmiTypes /w:root\wmi\EventTrace\MSNT_SystemTrace
");
                Environment.Exit(0);
            }

            foreach (string arg in args)
            {
                string name = arg.Substring(0, 3);
                string value = arg.Substring(3);

                switch (name)
                {
                    case "/a:":
                        if (!String.IsNullOrEmpty(assembly))
                        {
                            Console.WriteLine("The assembly switch /a: occurs more than once.");
                            Environment.Exit(1);
                        }
                        assembly = value;
                        break;

                    case "/o:":
                        if (!String.IsNullOrEmpty(outputDirectory))
                        {
                            Console.WriteLine("The output directory switch /o: occurs more than once.");
                            Environment.Exit(1);
                        }
                        outputDirectory = value;
                        break;

                    case "/m:":
                        string[] manifests;
                        string manifestDir = Path.GetDirectoryName(value);
                        if (String.IsNullOrEmpty(manifestDir))
                        {
                            manifests = Directory.GetFiles(".", value);
                        }
                        else
                        {
                            manifests = Directory.GetFiles(
                                manifestDir,
                                Path.GetFileName(value));
                        }

                        foreach (string manifest in manifests)
                        {
                            string content = File.ReadAllText(manifest);
                            Dictionary<string, string> code = ManifestParser.Parse(content);

                            foreach (string provider in code.Keys)
                            {
                                generated.Add(provider, code[provider]);
                            }
                        }
                        break;

                    case "/e:":
                        string[] etlFiles;
                        string etlDir = Path.GetDirectoryName(value);
                        if (String.IsNullOrEmpty(etlDir))
                        {
                            etlFiles = Directory.GetFiles(".", value);
                        }
                        else
                        {
                            etlFiles = Directory.GetFiles(
                                etlDir,
                                Path.GetFileName(value));
                        }

                        foreach (string etlFile in etlFiles)
                        {
                            string[] etlManifests = ManifestParser.ExtractFromTrace(etlFile);

                            if (etlManifests != null && etlManifests.Length > 0)
                            {
                                int i = 0;
                                foreach (string content in etlManifests)
                                {
                                    Dictionary<string, string> code = ManifestParser.Parse(content);

                                    foreach (string provider in code.Keys)
                                    {
                                        generated.Add(provider, code[provider]);
                                    }

                                    // Write the manifest text file
                                    using (TextWriter wr =
                                        new StreamWriter(Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(etlFile) + "_" + i.ToString() + ".man")))
                                    {
                                        wr.Write(content);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No manifest found in file:{0}", etlFile);
                            }
                        }
                        break;

                    case "/t:":
                        string[] tmfs;
                        string tmfDir = Path.GetDirectoryName(value);
                        if (String.IsNullOrEmpty(tmfDir))
                        {
                            tmfs = Directory.GetFiles(".", value);
                        }
                        else
                        {
                            tmfs = Directory.GetFiles(
                                tmfDir,
                                Path.GetFileName(value));
                        }

                        foreach (string tmf in tmfs)
                        {
                            Console.WriteLine(tmf);
                            string provider = Path.GetFileNameWithoutExtension(tmf);
                            string code = TmfParser.Parse(tmf);
                            generated.Add(provider, code);
                        }
                        break;

                    case "/p:":
                        string[] perfTraces;
                        string perfDir = Path.GetDirectoryName(value);
                        if (String.IsNullOrEmpty(perfDir))
                        {
                            perfTraces = Directory.GetFiles(".", value);
                        }
                        else
                        {
                            perfTraces = Directory.GetFiles(
                                perfDir,
                                Path.GetFileName(value));
                        }

                        foreach (string perfTrace in perfTraces)
                        {
                            Console.WriteLine(perfTrace);
                            Dictionary<string, string> code = PerfCounterParser.Parse(perfTrace);

                            foreach (string provider in code.Keys)
                            {
                                generated.Add(provider, code[provider]);
                            }
                        }
                        break;

                    case "/w:":
                        Console.WriteLine("The WMI switch /w: is not yet implemented.");
                        Environment.Exit(2);
                        break;

                    default:
                        Console.WriteLine("Unknown switch " + arg);
                        Environment.Exit(1);
                        break;
                }
            }
        }

        private static void OutputCode()
        {
            foreach (string provider in generated.Keys)
            {
                using (TextWriter wr = new StreamWriter(Path.Combine(outputDirectory, provider + ".cs")))
                {
                    wr.Write(generated[provider]);
                }
            }
        }
    }
}
