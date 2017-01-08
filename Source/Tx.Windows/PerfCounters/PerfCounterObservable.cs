// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive.Linq;

namespace Tx.Windows
{
    using System.Reactive.Disposables;

    public static class PerfCounterObservable
    {
        public static IObservable<PerformanceSample> FromFile(string perfTrace)
        {
            return Observable.Create<PerformanceSample>(o => new PerfCounterFileReader(o, perfTrace));
        }

        public static IObservable<PerformanceSample> FromRealTime(TimeSpan samplingRate, params string[] counterPaths)
        {
            return Observable
                .Create<PerformanceSample>(
                    o =>
                        {
                            PerfCounterRealTimeProbe probe;
                            try
                            {
                                probe = new PerfCounterRealTimeProbe(o, samplingRate, counterPaths);
                            }
                            catch (Exception error)
                            {
                                o.OnError(error);
                                return Disposable.Empty;
                            }

                            return probe;
                        });
        }
    }
}