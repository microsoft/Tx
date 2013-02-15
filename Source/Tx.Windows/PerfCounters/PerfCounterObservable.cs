using System;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public static class PerfCounterObservable
    {
        public static IObservable<PerformanceSample> FromFile(string perfTrace)
        {
            return Observable.Create<PerformanceSample>(o => new PerfCounterFileReader(o, perfTrace));
        }
    }
}
