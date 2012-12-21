using System;
using System.Reactive.Linq;
using Tx.Windows;

namespace TxSamples.EtwRaw
{
    class Program
    {
        static void Main()
        {
            IObservable<EtwNativeEvent> etl = EtwObservable.FromFiles(@"HTTP_Server.etl");
            etl.Count().Subscribe(Console.WriteLine);

            Console.ReadLine();
        }
    }
}
