// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Tx.Windows
{
    public sealed class PerfCounterFileReader : IDisposable
    {
        private readonly bool _binaryLog;
        private readonly List<PerfCounterInfo> _counters = new List<PerfCounterInfo>();
        private readonly IObserver<PerformanceSample> _observer;
        private readonly PdhQueryHandle _query;
        private bool _firstMove = true;

        public PerfCounterFileReader(IObserver<PerformanceSample> observer, string file)
        {
            _observer = observer;
            string extension = Path.GetExtension(file);
            if (extension != null) _binaryLog = extension.ToLowerInvariant() == ".blg";

            string[] counterPaths = PdhUtils.GetCounterPaths(file);

            PdhStatus status = PdhNativeMethods.PdhOpenQuery(file, IntPtr.Zero, out _query);
            PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

            foreach (string counter in counterPaths)
            {
                AddCounter(counter);
            }

            Read();
        }

        public void Dispose()
        {
            foreach (PerfCounterInfo counterInfo in _counters)
            {
                counterInfo.Dispose();
            }

            _query.Dispose();
        }

        public void Read()
        {
            try
            {
                if (_firstMove)
                {
                    // some counters need two samples to calculate their value
                    // so skip a sample to make sure there are no further complications
                    PdhNativeMethods.PdhCollectQueryData(_query);
                    _firstMove = false;
                }

                while (true)
                {
                    long time;
                    PdhStatus status = PdhNativeMethods.PdhCollectQueryDataWithTime(_query, out time);
                    if (status == PdhStatus.PDH_NO_MORE_DATA)
                        break;

                    if (status == PdhStatus.PDH_NO_DATA)
                        if (_binaryLog)
                            continue;
                        else
                            break;

                    PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);
                    DateTime timestamp = DateTime.FromFileTimeUtc(time);

                    foreach (PerfCounterInfo counterInfo in _counters)
                    {
                        ProduceCounterSamples(counterInfo, timestamp);
                    }
                }

                _observer.OnCompleted();
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
            }
        }

        private void ProduceCounterSamples(PerfCounterInfo counterInfo, DateTime timestamp)
        {
            uint bufferSize = 0;
            uint bufferCount;

            PdhStatus status = PdhNativeMethods.PdhGetFormattedCounterArray(
                counterInfo.Handle,
                PdhFormat.PDH_FMT_DOUBLE,
                ref bufferSize,
                out bufferCount,
                IntPtr.Zero);
            PdhUtils.CheckStatus(status, PdhStatus.PDH_MORE_DATA);

            var buffer = new byte[bufferSize];
            unsafe
            {
                fixed (byte* pb = buffer)
                {
                    status = PdhNativeMethods.PdhGetFormattedCounterArray(
                        counterInfo.Handle,
                        PdhFormat.PDH_FMT_DOUBLE,
                        ref bufferSize,
                        out bufferCount,
                        (IntPtr) pb);
                    if (status == PdhStatus.PDH_INVALID_DATA
                        || status == PdhStatus.PDH_CALC_NEGATIVE_VALUE
                        || status == PdhStatus.PDH_CALC_NEGATIVE_DENOMINATOR
                        || status == PdhStatus.PDH_CALC_NEGATIVE_TIMEBASE)
                    {
                        var sample = new PerformanceSample(counterInfo, timestamp, double.NaN);
                        _observer.OnNext(sample);
                        return;
                    }

                    PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

                    var items = (PDH_FMT_COUNTERVALUE_ITEM*) pb;
                    for (int i = 0; i < bufferCount; i++)
                    {
                        PDH_FMT_COUNTERVALUE_ITEM* item = items + i;
                        var sample = new PerformanceSample(counterInfo, timestamp, item->FmtValue.doubleValue);

                        _observer.OnNext(sample);
                    }
                }
            }
        }

        private void AddCounter(string counterPath)
        {
            PdhCounterHandle counter;
            PdhStatus status = PdhNativeMethods.PdhAddCounter(_query, counterPath, IntPtr.Zero, out counter);
            if (status == PdhStatus.PDH_ENTRY_NOT_IN_LOG_FILE)
                return;

            PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

            var counterInfo = new PerfCounterInfo(counterPath, counter);
            _counters.Add(counterInfo);
        }

        ~PerfCounterFileReader()
        {
            Dispose();
        }
    }
}