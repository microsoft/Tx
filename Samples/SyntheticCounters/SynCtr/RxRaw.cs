// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;

namespace SynCtr
{
    // This is implementation of Rx query on raw events, 
    // without transforming them to typed objects

    internal class RxRaw
    {
        private static IDisposable _subscription;
        private static IObservable<EtwNativeEvent> _raw;


        public static void ListenWintQueryOnEtwNativeEvent()
        {
            Console.WriteLine("----- Listening with Tx-EtwObservable and Rx query -----");

            _raw = EtwObservable.FromSession(Baseline.SessionName);

            UInt32 pid = 0;
            UInt32 size = 0;
            UInt32 daddr = 0;

            var timeSource = new TimeSource<EtwNativeEvent>(_raw, e => e.TimeStamp);

            var toStackVars = timeSource.Do(e => // this copies the variables on the stack
                {
                    pid = e.ReadUInt32(); // skip PID
                    size = e.ReadUInt32();
                    daddr = e.ReadUInt32();
                });

            var x = from window in toStackVars.Window(TimeSpan.FromSeconds(1))
                    from stats in
                        (from packet in window
                         group packet by daddr
                         into g
                         from total in g.Sum(p => size)
                         select new
                             {
                                 address = new IPAddress(g.Key).ToString(),
                                 received = total
                             })
                        .ToList()
                    select stats.OrderBy(s => s.address);

            _subscription = x.Subscribe(v =>
                {
                    Console.WriteLine("--- {0} ---", DateTime.Now);
                    foreach (var s in v)
                        Console.WriteLine("{0, -15} {1,-10:n0} ", s.address, s.received);
                    Console.WriteLine();
                });

            timeSource.Connect();

            Console.ReadLine();
            Console.WriteLine(pid); // prevent the compiler to optimize this away
            _subscription.Dispose();
            timeSource.Dispose();
        }

        public static void ListenWintUnsafeClass()
        {
            Console.WriteLine("----- Listening with Unsafe wrapper class and Rx query -----");

            // this is the approach used by TraceEvent
            // http://blogs.msdn.com/b/dotnet/archive/2013/08/15/announcing-traceevent-monitoring-and-diagnostics-for-the-cloud.aspx
            // - It works in this case and provides better performance
            // - In general means the user must think which data to copy as the first step in the query
            //   For example in query that joins begin and end event, we can't stop ETW from overwriting the buffer before matching end arrives

            var instance = new RecvV4();

            _raw = EtwObservable.FromSession(Baseline.SessionName);
            var timeSource = new TimeSource<EtwNativeEvent>(_raw, e => e.TimeStamp);

            var received = timeSource.Select(e =>
                {
                    unsafe
                    {
                        instance.userData = (byte*) e.UserData.ToPointer();
                    }
                    return instance;
                });

            var x = from window in received.Window(TimeSpan.FromSeconds(1), timeSource.Scheduler)
                    from stats in
                        (from packet in window
                         group packet by packet.daddr into g
                         from total in g.Sum(p => p.size)
                         select new
                         {
                             address = new IPAddress(g.Key).ToString(),
                             received = total
                         })
                            .ToList()
                    select stats.OrderBy(s => s.address);

            _subscription = x.Subscribe(v =>
            {
                //Console.WriteLine("--- {0} ---", DateTime.Now);
                //foreach (var s in v)
                //    Console.WriteLine("{0, -15} {1,-10:n0} ", s.address, s.received);
                //Console.WriteLine();
            });
            timeSource.Connect();

            Console.ReadLine();
            _subscription.Dispose();
            timeSource.Dispose();
        }
    }

    unsafe class RecvV4
    {
        public byte* userData;

        public uint PID { get { return *((uint*)(userData)); } }
        public uint size { get { return *((uint*)(userData + 4)); } }
        public uint daddr { get { return *((uint*)(userData + 8)); } }
    }
}
