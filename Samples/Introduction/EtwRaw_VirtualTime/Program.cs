using System;
using System.Reactive;
using System.Reactive.Linq;
using Tx.Windows;


namespace TxSamples.EtwRaw_VirtualTime
{
    class Program
    {
        static void Main()
        {
            IObservable<EtwNativeEvent> etl = EtwObservable.FromFiles(@"HTTP_Server.etl");

            var timeSource = new TimeSource<EtwNativeEvent>(etl, e => e.TimeStamp);

            var countPerWindow = from window in timeSource.Window(TimeSpan.FromSeconds(5), timeSource.Scheduler)
                    from Count in window.Count()
                    select Count;

            var withTime = countPerWindow.Timestamp(timeSource.Scheduler);

            withTime.Subscribe(ts => Console.WriteLine("{0} {1}", ts.Timestamp, ts.Value));
            timeSource.Connect();

            Console.ReadLine();
        }
    }
}
