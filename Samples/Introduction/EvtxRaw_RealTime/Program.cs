using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using Tx.Windows;

namespace TxSamples.EvtxRaw
{
    class Program
    {
        static void Main()
        {
            IObservable<EventRecord> evtx = EvtxObservable.FromLog("Application");
            evtx.Subscribe(e=>Console.WriteLine(e.FormatDescription()));
           
            EventLog log = new EventLog("Application");
            log.Source = "EvtxRaw_RealTime";  
 
            for (int i = 0; i < 5; i++)
            {
                log.WriteEntry("test " + i);
            }

            Console.ReadLine();
        }
    }
}
