namespace Microsoft.Etw
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Tx.Windows;
    
    class Program
    {
        static string assembly = "";
        static string outputDirectory = "";
        static Dictionary<string, string> generated = new Dictionary<string, string>();

        static void Main(string[] args)
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
                AssemblyBuilder.OutputAssembly(generated, target);
            }
        }

        static void Parse(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
@"Usage: 
    EtwEventTypeGen [/o:dir] [/a:name] [/m:file] [/t:file] [/w:path]

Switches:
    /o:dir       Directory for the output. 
                 If missing, the output is written to the current directory

    /a:name      Generate Assembly. 
                 If missing the output is C# files.

    /m:manifest	 Input from manifest(s)

    /t:tmf       Input from TMF file(s)

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
                            var code = ManifestParser.Parse(content);

                            foreach (string provider in code.Keys)
                            {
                                generated.Add(provider, code[provider]);
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

        static void OutputCode()
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

