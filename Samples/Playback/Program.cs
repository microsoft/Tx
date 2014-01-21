// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace TxSamples
{
    class Program
    {
        static void Main()
        {
            // Structured mode
            GetObservable();
            VirtualTime();
            Get2Observables();
            Format2();
            Count2(); 

            // Timeline mode
            FormatAll();
            CountAll();
            Count2And12();
            CountAllTwoFiles();
            CountAcrossHierarchies();
            Count5SecWindow();
            PerfCounterAverageAndDeviation();
        }

        #region Structured Mode

        static void GetObservable()
        {
            // This sample illustrates parsing single input file and transforming only the events that are of interest
            Console.WriteLine("----- GetObservable -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            playback.GetObservable<Parse>().Subscribe(p => Console.WriteLine(p.Url));

            playback.Run();

            //Here: 
            //  - HTTP_Server.etl is trace from HTTP.sys (the bottom layer of IIS)
            //  - The type Parse was generated from the ETW manifest description of the first event IIS traces for each request (EventId = 2)
            //  - Subscribe just wires-up the processing of Parse events, and nothing shows on the console yet. 
            //    Reading the file happens within Run().
        }

        static void VirtualTime()
        {
            // This sample illustrates the concept of Virtual Time, constructed as per timestamps of the events in the file 
            Console.WriteLine("----- VirtualTime -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            playback.GetObservable<Parse>().Subscribe(p => Console.WriteLine("{0} {1}",
                playback.Scheduler.Now.DateTime, p.Url));

            playback.Run();
        }

        static void Get2Observables()
        {
            // This sample shows how to subscribe to two types of events, and process the occurences (=instances) reading the file once
            Console.WriteLine("----- Get2Observables -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            playback.GetObservable<Parse>().Subscribe(p => Console.WriteLine("begin {0} {1}", p.Header.ActivityId, p.Url));
            playback.GetObservable<FastSend>().Subscribe(fs => Console.WriteLine("end   {0} {1}", fs.Header.ActivityId, fs.HttpStatus));

            playback.Run();

            // Here we first wire-up the two processing pipelines, and then we call Run(). 
            // Reading the file, Playback ignores all other events except Parse (EventId=2) and FastSend (EventId=12). 
            // These are  pushed into the corresponding IObservables in order of occurence. 
            //
            // Typically we see Parse, followed by FastSend with the same ActivityId.
        }

        static void Format2()
        {
            // This sample shows how to subscribe to format just two event types of events
            Console.WriteLine("----- Format2 -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            playback.GetObservable<Parse>().Subscribe(Console.WriteLine);    // Implicit ToString()
            playback.GetObservable<FastSend>().Subscribe(Console.WriteLine); // Implicit ToString()

            playback.Run();
        }

        static void Count2()
        {
            // This sample counts two kinds of events:
            Console.WriteLine("----- Count2 -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            int cParse = 0;
            int cFastSend = 0;

            playback.GetObservable<Parse>().Subscribe(c => cParse++);
            playback.GetObservable<FastSend>().Subscribe(c=> cFastSend++);

            playback.Run();
            Console.WriteLine("Parse: {0}, FastSend: {1}, Total: {2}",
                cParse, cFastSend, cParse + cFastSend);

            // Some questionsare difficult to express as structured queries
            // Although possible in specific cases, the expressions are fragile (the user has to  list all event types),
        }

        #endregion

        #region Timeline Mode

        static void FormatAll()
        {
            // This sample shows how to format all events, similar to existing tools like TraceVwr, TraceFmt, etc.
            // Formatting means ignoring types and displaying human-readable text in order of occurence
            Console.WriteLine("----- Formatting -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            playback.KnownTypes = typeof(Parse).Assembly.GetTypes();
            var all = playback.GetObservable<SystemEvent>()
                .Select(e => e.ToString());

            all.Subscribe(Console.WriteLine);

            playback.Run();
        }
        
        static void CountAll()
        {
            // Subscribing to SystemEvent means "all ETW events"
            Console.WriteLine("----- CountAll -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            var all = playback.GetObservable<SystemEvent>();
            all.Count().Subscribe(Console.WriteLine);

            playback.Run();
        }

        static void Count2And12()
        {
            // This sample shows how to use SystemEvent to obtain the same result as in Count2
            // by using SystemEvent instead of generated classes (2 is Parse, 12 is FastSend)
            Console.WriteLine("----- Count2And12 -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            var filtered = playback.GetObservable<SystemEvent>()
                .Where(e=>e.Header.EventId == 2 || e.Header.EventId == 12);

            filtered.Count().Subscribe(Console.WriteLine);

            playback.Run();
        }

        static void CountAllTwoFiles()
        {
            // The file HTTP_Server.evtx is Windows Event Log, obtained by converting HTTP_Server.etl
            // It contains the same exact events, so let's count total # of events in the two files
            Console.WriteLine("----- CountAllTwoFiles -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");
            playback.AddLogFiles(@"HTTP_Server.evtx");

            var all = playback.GetObservable<SystemEvent>();
            all.Count().Subscribe(Console.WriteLine);

            playback.Run();
        }

        static void CountAcrossHierarchies()
        {
            // The method GetAll works across event hierarchies - e.g. the PerfCounterSample events don't inherit from SystemEvent
            Console.WriteLine("----- CountAllThreeFiles -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");
            playback.AddPerfCounterTraces(@"BasicPerfCounters.blg");

            var types = new List<Type>(typeof(Parse).Assembly.GetTypes());
            types.Add(typeof(PerformanceSample));

            var all = playback.GetAll(types.ToArray());
            all.Count().Subscribe(Console.WriteLine);

            playback.Run();
        }

        static void Count5SecWindow()
        {
            // Counting all events, every 5 sec window in Virtual Time
            Console.WriteLine("----- Count5SecWindow -----");

            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            var all = playback.GetObservable<SystemEvent>();

            var countPerWindow = from window in all.Window(TimeSpan.FromSeconds(5), playback.Scheduler)
                                 from Count in window.Count()
                                 select Count;

            countPerWindow.Subscribe(Console.WriteLine);

            playback.Run();
        }

        static void PerfCounterAverageAndDeviation()
        {
            var playback = new Playback();
            playback.AddPerfCounterTraces(@"BasicPerfCounters.blg");

            var procTimeTotal = from ps in playback.GetObservable<PerformanceSample>()
                                where ps.CounterSet == "Processor" && ps.CounterName == "% Processor Time" && ps.Instance == "_Total"
                                select ps;

            var powerSumBases = from ps in procTimeTotal
                                select new
                                {
                                    s0_base = 1,
                                    s1_base = ps.Value,
                                    s2_base = ps.Value * ps.Value
                                };

            var powerSums =
                from window in
                    powerSumBases.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), playback.Scheduler)
                from a in window.Aggregate(new { s0 = 0, s1 = 0.0, s2 = 0.1 }, (acc, point) => new
                {
                    s0 = acc.s0 + point.s0_base,
                    s1 = acc.s1 + point.s1_base,
                    s2 = acc.s2 + point.s2_base
                })
                select a;

            var avgAndDeviation = from ps in powerSums
                                  select new
                                  {
                                      Average = ps.s1 / ps.s0,
                                      Deviation = Math.Sqrt((ps.s0 * ps.s2 - ps.s1 * ps.s1) / (ps.s0 * ps.s0 - 1))
                                  };

            avgAndDeviation.Subscribe(value => Console.WriteLine("Average: {0}, Deviation: {1}", value.Average, value.Deviation));
            playback.Run();
        }

        #endregion 
    }
}
