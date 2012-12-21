using System;
using System.Reactive.Linq;
using Microsoft.SqlServer.XEvent.Linq;
using Tx.SqlServer;

namespace TxSamples.XeObservalbe
{
    class Program
    {
        static void Main()
        {
            IObservable<PublishedEvent> xe = XeObservable.FromFiles(@"gatewaysample*.xel");
            xe.Count().Subscribe(Console.WriteLine);

            Console.ReadLine();
        }
    }
}
