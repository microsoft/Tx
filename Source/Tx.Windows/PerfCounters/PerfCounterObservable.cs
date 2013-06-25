// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

        public static IObservable<PerformanceSample> FromRealTime(TimeSpan samplingRate, params string[] counterPaths)
        {
            return Observable.Create<PerformanceSample>(o => new PerfCounterRealTimeProbe(o, samplingRate, counterPaths));
        }
    }
}