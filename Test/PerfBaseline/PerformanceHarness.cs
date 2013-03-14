// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PerformanceBaseline
{
    public class PerformanceHarness
    {
        static List<string> _queryTechnologies;
        static List<string> _inputOptions;
        static List<string> _suites;
        static List<string> _tests;

        static int _runCount = 1;

        static TextWriter _writer;
        static public void Execute(string[] args, Type[] types)
        {
            Parse(args);

            _writer = File.CreateText("PerformanceBaseline.csv");
            _writer.WriteLine("QT  , Input     , Suite                , Test                   , Duration");

            var allTests = from t in types
                           where t.GetCustomAttributes(typeof(PerformanceTestSuiteAttribute), false).Length > 0
                           from test in GetTests(t)
                           select new TestInfo
                           {
                               QueryTechnology = GetAttribute<PerformanceTestSuiteAttribute>(t).QueryTechnology,
                               SuiteName = GetAttribute<PerformanceTestSuiteAttribute>(t).Name,
                               TestName = test.Name,
                               SuiteType = t,
                               TestMethod = test
                           };

            var testsToRun = from t in allTests
                             where _queryTechnologies.Count == 0 || _queryTechnologies.Contains(t.QueryTechnology)
                             where _suites.Count == 0 || (from s in _suites where t.SuiteName.StartsWith(s) select s).Any()
                             where _tests.Count == 0 || (from ts in _tests where t.TestName.StartsWith(ts) select ts).Any()
                             select t;

            for (int i = 0; i < _runCount; i++)
            {
                Console.WriteLine("================= {0} Run {1} of {2} ==================", DateTime.Now, i + 1, _runCount);
                Console.WriteLine();
                Console.WriteLine("QT   Suite                 Test                     Duration");
                Console.WriteLine();

                foreach (var t in testsToRun)
                {
                    RunTest(t);
                }
                Console.WriteLine();
            }
            _writer.Flush();
        }

        static void RunTest(TestInfo t)
        {
            Console.Write("{0,-4} {1,-21} {2,-23}  ",
                t.QueryTechnology,
                t.SuiteName,
                t.TestName);

            var suite = (IPerformanceTestSuite)Activator.CreateInstance(t.SuiteType, new object[] { });

            double duration = suite.RunTestcase(t.TestMethod).TotalSeconds;

            Console.WriteLine(duration);

            _writer.WriteLine("{0,-4}, {1,-21}, {2,-23}, {3}",
                t.QueryTechnology,
                t.SuiteName,
                t.TestName,
                duration);
        }

        static T GetAttribute<T>(ICustomAttributeProvider provider)
        {
            return (T)(provider.GetCustomAttributes(typeof(T), false))[0];
        }

        static MethodInfo[] GetTests(Type suite)
        {
            MethodInfo[] methods = suite.GetMethods();
            var tests = from m in methods where m.GetCustomAttributes(typeof(PerformanceTestCaseAttribute), false).Length > 0 select m;

            return tests.ToArray();
        }

        static void Parse(string[] args)
        {
            if (args.Length == 0)
                PrintUsageAndExit();

            _queryTechnologies = new List<string>();
            _inputOptions = new List<string>();
            _suites = new List<string>();
            _tests = new List<string>();

            foreach (string arg in args)
            {
                string name = arg.Substring(0, 3);
                string value = arg.Substring(3);

                switch (name)
                {
                    case "/q:":
                        _queryTechnologies.Add(value);
                        break;

                    case "/i:":
                        _inputOptions.Add(value);
                        break;

                    case "/s:":
                        _suites.Add(value);
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

        static void PrintUsageAndExit()
        {
            Console.WriteLine(
@"Usage: 
    PerformanceBaseline [/t:testpath] [/r:runcount] 

Switches:
    /q:queryTechnology Test given implementation {Code, Rx,...)
    /s:testsuite       Run suites starting with this string
    /t:test            Run tests starting with this string 
    /r:runcount        How many times to run

Examples:

    PerformanceBaseline /q:Rx /s:MSNT_SystemTrace /t:\""Context Switch\""
    PerformanceBaseline /s:Cluster /r:10
");
            Environment.Exit(0);
        }

        class TestInfo
        {
            public string QueryTechnology;
            public string SuiteName;
            public string TestName;
            public Type SuiteType;
            public MethodInfo TestMethod;
        };

    }
}

