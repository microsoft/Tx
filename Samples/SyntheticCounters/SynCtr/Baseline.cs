// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using Tx.Windows;

namespace SynCtr
{
    // This is implementation of the same exact semantics as the query, but using imperative code
    // Here we don't do allocations per event
    // Some allocations happen once per second

    class Baseline
    {
        public const string SessionName = "tcp";
        static Guid ProviderId = new Guid("{7dd42a49-5329-4832-8dfd-43d979153a88}");
        static IDisposable _subscription;

        private static IObservable<EtwNativeEvent> _raw;
        private static object _lock = new object();
        private static Dictionary<uint, StatisticsBucket> _statistics;
        private static Timer _timer;

        public static void ListenWithImperativeCode()
        {
            Console.WriteLine("----- Listening with Tx-EtwObservable and imperative code -----");

            _statistics = new Dictionary<uint, StatisticsBucket>();
            _timer = new Timer(OnTimer, null, 1000, 1000);
            _raw = EtwObservable.FromSession(SessionName);

            using (_raw.Subscribe(CustomCallback))
            {
                Console.ReadLine(); 
            }
        }

        static void CustomCallback(EtwNativeEvent evt)
        {
            if (evt.Id != 11)
                return;

            evt.ReadUInt32(); // skip PID
            uint size = evt.ReadUInt32();
            uint daddr = evt.ReadUInt32();

            lock (_lock)
            {
                StatisticsBucket bucket = null;
                if (!_statistics.TryGetValue(daddr, out bucket))
                {
                    bucket = new StatisticsBucket { Total = size };
                    _statistics.Add(daddr, bucket);
                    return;
                }

                bucket.Total += size;                
            }
        }

        static void OnTimer(object state)
        {
            lock (_lock)
            {
                if (_statistics.Count == 0)
                    return;

                foreach (KeyValuePair<uint, StatisticsBucket> pair in _statistics)
                {
                    Console.WriteLine("{0, -15} {1,-10:n0} ", 
                        new IPAddress(pair.Key).ToString(), 
                        pair.Value.Total);
                }
                Console.WriteLine();
       
                _statistics = new Dictionary<uint, StatisticsBucket>();
            }
        }


        public static void StartSession()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                throw new Exception("To use ETW real-time session you must be administrator");

            Process logman = Process.Start("logman.exe", "stop " + SessionName + " -ets");
            logman.WaitForExit();

            logman = Process.Start("logman.exe", "create trace " + SessionName + " -rt -nb 2 2 -bs 1024 -p {" + ProviderId + "} 0xffffffffffffffff -ets");
            logman.WaitForExit();
        }
    }

    internal class StatisticsBucket
    {
        public double Total;
    }
}
