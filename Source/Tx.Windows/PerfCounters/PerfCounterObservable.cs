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
    }
}
