// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Tx.Windows
{
    public sealed class PerfCounterFileReader : IDisposable
    {
        IObserver<PerformanceSample> _observer;
        PdhQueryHandle _query;
        List<PerfCounterInfo> _counters = new List<PerfCounterInfo>();
        bool _firstMove = true;
        bool _binaryLog;

        public PerfCounterFileReader(IObserver<PerformanceSample> observer, string file)
        {
            _observer = observer;
            _binaryLog = Path.GetExtension(file).ToLowerInvariant() == ".blg";

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
            PdhStatus status;
            try
            {
                if (_firstMove)
                {
                    // some counters need two samples to calculate their value
                    // so skip a sample to make sure there are no further complications
                    status = PdhNativeMethods.PdhCollectQueryData(_query);
                    _firstMove = false;
                }

                while (true)
                {
                    long time;
                    status = PdhNativeMethods.PdhCollectQueryDataWithTime(_query, out time);
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

        void ProduceCounterSamples(PerfCounterInfo counterInfo, DateTime timestamp)
        {
            uint bufferSize = 0;
            uint bufferCount = 0;

            PdhStatus status = PdhNativeMethods.PdhGetFormattedCounterArray(
                counterInfo.Handle,
                PdhFormat.PDH_FMT_DOUBLE,
                ref bufferSize,
                out bufferCount,
                IntPtr.Zero);
            PdhUtils.CheckStatus(status, PdhStatus.PDH_MORE_DATA);

            byte[] buffer = new byte[bufferSize];
            unsafe
            {
                fixed (byte* pb = buffer)
                {
                    status = PdhNativeMethods.PdhGetFormattedCounterArray(
                        counterInfo.Handle,
                        PdhFormat.PDH_FMT_DOUBLE,
                        ref bufferSize,
                        out bufferCount,
                        (IntPtr)pb);
                    if (status == PdhStatus.PDH_INVALID_DATA
                        || status == PdhStatus.PDH_CALC_NEGATIVE_VALUE
                        || status == PdhStatus.PDH_CALC_NEGATIVE_DENOMINATOR
                        || status == PdhStatus.PDH_CALC_NEGATIVE_TIMEBASE)
                    {
                        PerformanceSample sample = new PerformanceSample(counterInfo, timestamp, double.NaN);
                        _observer.OnNext(sample);
                        return;
                    }

                    PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

                    PDH_FMT_COUNTERVALUE_ITEM* items = (PDH_FMT_COUNTERVALUE_ITEM*)pb;
                    for (int i = 0; i < bufferCount; i++)
                    {
                        PDH_FMT_COUNTERVALUE_ITEM* item = items + i;
                        PerformanceSample sample = new PerformanceSample(counterInfo, timestamp, item->FmtValue.doubleValue);

                        _observer.OnNext(sample);
                    }
                }
            }
        }

        void AddCounter(string counterPath)
        {
            PdhCounterHandle counter;
            PdhStatus status = PdhNativeMethods.PdhAddCounter(_query, counterPath, IntPtr.Zero, out counter);
            if (status == PdhStatus.PDH_ENTRY_NOT_IN_LOG_FILE)
                return;

            PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

            var  counterInfo = new PerfCounterInfo(counterPath, counter);
            _counters.Add(counterInfo);
        }

        ~PerfCounterFileReader()
        {
            Dispose();
        }
    }
}
