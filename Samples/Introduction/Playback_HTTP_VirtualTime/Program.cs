using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace TxSamples.Playback_HTTP_VirtualTime
{
    class Program
    {
        static void Main()
        {
            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");

            IObservable<Deliver> startEvents = playback.GetObservable<Deliver>();
            IObservable<FastResp> endEvents = playback.GetObservable<FastResp>();

            var requests = from start in startEvents
                           from end in endEvents
                                .Where(e => start.Header.ActivityId == e.Header.ActivityId)
                                .Take(TimeSpan.FromSeconds(1), playback.Scheduler) // <-- Playback virtual time!
                                .Take(1)
                           select new
                           {
                               start.Url,
                               end.StatusCode,
                               Duration = end.Header.Timestamp - start.Header.Timestamp
                           };

            var statistics = from window in requests.Window(TimeSpan.FromSeconds(5), playback.Scheduler)
                             from stats in
                                 (   // calculate statistics within one window
                                     from request in window
                                     group request by new
                                         {
                                             Milliseconds = Math.Ceiling(request.Duration.TotalMilliseconds * 10) / 10,
                                             request.Url
                                         } into g
                                     from Count in g.Count()
                                     select new
                                     {
                                         g.Key.Url,
                                         g.Key.Milliseconds,
                                         Count
                                     })
                                     .ToList() 
                             select stats;

            statistics.Subscribe(b =>
                {
                    Console.WriteLine("--------------------------");
                    foreach (var s in b.OrderBy(s => s.Milliseconds)) // <-- LINQ to Objects!
                    {
                        Console.WriteLine(s);
                    }
                });

            playback.Run();

            Console.ReadLine();
        }
    }
}
