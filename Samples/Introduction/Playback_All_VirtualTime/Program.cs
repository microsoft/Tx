// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;

namespace TxSamples
{
    class Program
    {
        static void Main()
        {
            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");
            playback.AddLogFiles(@"HTTP_Server.evtx");

            IObservable<SystemEvent> all = playback.GetObservable<SystemEvent>();

            var counts = from window in all.Window(TimeSpan.FromSeconds(5), playback.Scheduler)
                    from Count in window.Count()
                    select Count;

            var withTime = counts.Timestamp(playback.Scheduler);

            using (withTime.Subscribe(ts => Console.WriteLine("{0} {1}", ts.Timestamp, ts.Value)))
            {
                playback.Run();

                Console.ReadLine();
            }
        }
    }
}
