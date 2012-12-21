using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.SqlServer.XEvent.Linq;

namespace Tx.SqlServer
{
    public class XeObservable
    {
        public static IObservable<PublishedEvent> FromFiles(params string[] xelFiles)
        {
            if (xelFiles == null)
                throw new ArgumentNullException("xelFiles");

            // the reader did not work with relative paths? could not pass in "Playback.xel"?
            string[] fullPaths = (from f in xelFiles select Environment.CurrentDirectory + "\\" + f).ToArray();

            QueryableXEventData enumerable = new QueryableXEventData(fullPaths);

            return enumerable.ToObservable(ThreadPoolScheduler.Instance);
        }

        public static IObservable<PublishedEvent> FromTarget<TTarget>() where TTarget : XeSubject, new()
        {
            return new TTarget();
        }
    }
}
