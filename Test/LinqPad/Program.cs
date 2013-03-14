using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Diagnostics;
using System.IO;

namespace UITest
{
    class Program
    {
        static void PrintUsageAndExit()
        {
            Console.WriteLine(
@"Usage: LinqPadE2eTest [/l:linqPadPath] [/t:test] [/r:count]

linqPadPath: Path to LinqPad.exe or .lnk to launch LinqPad

test:        Path from the 'Tx (LINQ to Traces)' tree used as prefix 
             E.g. /t:WinRM\0 will run the queries starting with 0 in the WinRM folder

             /t can occur more than once
             To run all tests, don't specify any /t arguments

count:       How many times to run the specified tests");

            Environment.Exit(0);
        }

        static List<String> _tests;
        static string _linqPadPath = String.Empty;
        static LinqPadAutomation _linqPad;
        static TextWriter _errors;
        static int _runCount;
        static Stopwatch _stopwatch = Stopwatch.StartNew();
        static int _passCount = 0;
        static int _failCount = 0;


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsageAndExit();
            }

            Parse(args);

            if (!File.Exists(_linqPadPath))
            {
                Console.WriteLine("File not found {0}", _linqPadPath);
                Environment.Exit(1);
            }            

            _errors = File.CreateText("Errors.txt");

            for (int i = 0; i < _runCount; i++)
            {
                try
                {
                    _stopwatch = Stopwatch.StartNew();
                    _passCount = 0;
                    _failCount = 0;

                    RunEnabledTests();

                    _stopwatch.Stop();
                    Console.WriteLine();
                    Console.WriteLine(
                        "Run: {0} Passed: {1} Failed: {2} Run duration: {3}", 
                        i, 
                        _passCount, 
                        _failCount, 
                        _stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    ConsoleColor old = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ForegroundColor = old;

                    _errors.WriteLine(ex.Message);
                    _errors.WriteLine(ex.StackTrace);
                    _errors.WriteLine();
                    _errors.Flush();

                }
            }
        }


        static void Parse(string[] args)
        {
            if (args.Length == 0)
                PrintUsageAndExit();

            string desktopPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Desktop");
            _linqPadPath = Path.Combine(desktopPath, "LINQPad 4.lnk");

            _tests = new List<string>();

            foreach (string arg in args)
            {
                string name = arg.Substring(0, 3);
                string value = arg.Substring(3);

                switch (name)
                {
                    case "/l:":
                        _linqPadPath = value;
                        break;

                    case "/t:":
                        _tests.Add(value);
                        break;

                    case "/r:":
                        _runCount = int.Parse(value);
                        break;

                    default:
                        Console.WriteLine("Unknown switch " + arg);
                        Environment.Exit(1);
                        break;
                }
            }
        }

        static void RunEnabledTests()
        {
            using (_linqPad = new LinqPadAutomation(_linqPadPath, "Tx (LINQ to Traces)", "results"))
            {
                AutomationElementCollection groups = _linqPad.GetSampleGroups();

                foreach (AutomationElement group in groups)
                {
                    string groupName = group.GetName();
                    if (groupName == "HelloETW")
                        continue;

                    if (!IsGroupEnabled(groupName))
                        continue;

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(groupName);
                    AutomationElementCollection queries = _linqPad.GetSamplesInGroup(group);

                    foreach (AutomationElement query in queries)
                    {
                        if (query.GetName() == "_Readme")
                            continue;

                        if (!IsTestEnabled(groupName, query.GetName()))
                            continue;

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\t" + query.GetName() + "   ");

                        if (_linqPad.ExecuteQuery(group, query))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("PASS");
                            _passCount++;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("FAIL");
                            _failCount++;
                        }

                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        static bool IsGroupEnabled(string group)
        {
            if (_tests.Count == 0)
                return true;

            foreach (string prefix in _tests)
            {
                if (prefix.StartsWith(group) || group.StartsWith(prefix))
                    return true;
            }

            return false;
        }

        static bool IsTestEnabled(string group, string test)
        {
            if (_tests.Count == 0)
                return true;

            string path = group + "\"" + test;

            foreach (string prefix in _tests)
            {
                if (prefix.StartsWith(group) || group.StartsWith(prefix))
                    return true;
            }

            return false;
        }
    }
}
