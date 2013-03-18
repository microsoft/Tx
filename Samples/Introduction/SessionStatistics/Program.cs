// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Tx.Windows;

namespace SessionStatistics
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SessionStatistics <real-time session name> <seconds>");
                return;
            }

            string sessionName = args[0];
            int seconds = int.Parse(args[1]);

            Console.WriteLine("Measuring provider verbosity for session '{0}' for {1} seconds", sessionName, seconds);
            IObservable<EtwNativeEvent> session = EtwObservable.FromSession(sessionName);

            var timeSource = new TimeSource<EtwNativeEvent>(session, e => e.TimeStamp);

            var countPerWindow = from e in timeSource.Take(TimeSpan.FromSeconds(seconds), timeSource.Scheduler)
                                 group e by new {e.ProviderId} into g
                                 from total in g.Count()
                                 select new {Provider = g.Key, Count = total};

            ManualResetEvent evt = new ManualResetEvent(false);

            IDisposable output = countPerWindow.Subscribe(
                stat => Console.WriteLine("{0} {1}", stat.Provider, stat.Count), // OnNext
                e => Console.WriteLine(e.Message),                               // OnError
                () =>{ evt.Set();});                                             // OnCompleted

            IDisposable input  = timeSource.Connect();
            evt.WaitOne();

            output.Dispose();
            input.Dispose();
        }
    }
}
