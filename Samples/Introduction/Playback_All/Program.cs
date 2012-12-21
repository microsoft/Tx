using System;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;

namespace TxSamples.Playback_All
{
    class Program
    {
        static void Main()
        {
            Playback playback = new Playback();
            playback.AddEtlFiles(@"HTTP_Server.etl");
            playback.AddLogFiles(@"HTTP_Server.evtx");

            IObservable<SystemEvent> all = playback.GetObservable<SystemEvent>();

            all.Count().Subscribe(Console.WriteLine);

            playback.Run();

            Console.ReadLine();
        }
    }
}
