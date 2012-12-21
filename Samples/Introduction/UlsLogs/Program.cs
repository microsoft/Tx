using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

namespace UlsLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            Playback playback = new Playback();
            playback.AddUlsFiles(@"ULS.log");

            var s = playback.GetObservable<LeavingMonitoredScope>();
            s.Subscribe(e => Console.WriteLine(e.Scope));

            playback.Run();

            Console.ReadLine();
        }
    }
}
