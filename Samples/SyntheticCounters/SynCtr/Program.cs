// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Network;

namespace SynCtr
{
    class Program
    {
        static IDisposable _subscription;
        static Playback _playback;

        static void Main(string[] args)
        {
           if (args.Length == 0)
           {
               Console.WriteLine(
@"Usage: SynCtr [option]

Options are:
    Playback   - use playback and Rx query
    Raw        - Rx query on EtwNativeEvent-s (co copy to typed objects)
    Imperative - use imperative code, over EtwNativeEvent-s");
               return;
           }

           Baseline.StartSession();

            switch (args[0])
            {
                case "Playback":
                    ListenWithQuery();
                    break;

                case "Raw":
                    RxRaw.ListenWintQueryOnEtwNativeEvent();
                    break;

                case "Imperative":
                    Baseline.ListenWithImperativeCode();
                    break;

                case "Unsafe":
                    RxRaw.ListenWintUnsafeClass();
                    break;

                default:
                    throw new Exception("Unknown option " + args[0]);
            }
        }

        static void ListenWithQuery()
        {
            Console.WriteLine("----- Listening with Tx-Playback and Rx query -----");
            _playback = new Playback();
            _playback.AddRealTimeSession(Baseline.SessionName);

            var received = _playback.GetObservable<KNetEvt_RecvIPV4>();

            var x = from window in received.Window(TimeSpan.FromSeconds(1), _playback.Scheduler)
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
                Console.WriteLine("--- {0} ---", DateTime.Now);
                foreach (var s in v)
                    Console.WriteLine("{0, -15} {1,-10:n0} ", s.address, s.received);
                Console.WriteLine();
            });

            _playback.Start();
            Console.ReadLine();
            _subscription.Dispose();
        }
    }
}
