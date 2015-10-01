// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Tx.Windows
{
    public sealed class PerfCounterRealTimeProbe : PerfCounterReader
    {
        private bool _firstMove = true;
        private readonly Timer _timer;

        public PerfCounterRealTimeProbe(IObserver<PerformanceSample> observer, TimeSpan samplingRate, params string[] counterPaths)
            : base(observer)
        {
            PdhStatus status = PdhNativeMethods.PdhOpenQuery(null, IntPtr.Zero, out _query);
            PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

            for (int i=0; i<counterPaths.Length; i++)
            {
                AddCounter(counterPaths[i], i);
            }

            _timer = new Timer(OnTimer, null, TimeSpan.Zero, samplingRate);
        }

        public void OnTimer(object state)
        {
            try
            {
                if (_firstMove)
                {
                    // some counters need two samples to calculate their value
                    // so skip a sample to make sure there are no further complications
                    PdhNativeMethods.PdhCollectQueryData(_query);
                    _firstMove = false;
                    return;
                }

                long time;
                PdhStatus status = PdhNativeMethods.PdhCollectQueryDataWithTime(_query, out time);
                PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);
                DateTime timestamp = TimeUtil.FromFileTime(time);

                foreach (PerfCounterInfo counterInfo in _counters)
                {
                    ProduceCounterSamples(counterInfo, timestamp);
                }
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
            }
        }

        public override void Dispose()
        {
            _timer.Dispose();

            base.Dispose();
        }

        ~PerfCounterRealTimeProbe()
        {
            Dispose();
        }
    }
}