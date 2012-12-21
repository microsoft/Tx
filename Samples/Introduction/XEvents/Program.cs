using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.SqlServer.XEvent.Static;
using Microsoft.SqlServer.XEvent.Linq;
using Tx.SqlServer;

namespace XEvents
{
    class Program
    {
        static void Main()
        {
            Option1_TimeSource();
            //Option2_Playback();
        }

        static void Option1_TimeSource()
        {
            IObservable<PublishedEvent> obs = XeObservable.FromFiles(@"gatewaysample.xel");
            TimeSource<PublishedEvent> timeSource = new TimeSource<PublishedEvent>(obs, e => e.Timestamp);

            timeSource
                .Take(TimeSpan.FromMinutes(1), timeSource.Scheduler)
                .Where(e=>(double)e.Fields["LoginDurationMs"].Value > 100)
                .Subscribe(e =>
                {
                    Console.WriteLine("--- {0} {1}.{2} ---", e.Name, e.Timestamp, e.Timestamp.Millisecond);
                    foreach (PublishedEventField f in e.Fields)
                    {
                        Console.WriteLine("{0} = {1}", f.Name, f.Value);
                    }
                });

            timeSource.Connect();

            Console.ReadLine();
        }

        static void Option2_Playback()
        {
            Playback playback = new Playback();
            playback.AddXelFiles(@"gatewaysample.xel");

            IObservable<login_timing> logins = playback.GetObservable<login_timing>();

            logins
                .Take(TimeSpan.FromMinutes(1), playback.Scheduler)
                .Where(l => l.LoginDurationMs > 100)
                .Subscribe(l=>Console.WriteLine(l.LoginDurationMs));

            playback.Run();
        }
    }
}
