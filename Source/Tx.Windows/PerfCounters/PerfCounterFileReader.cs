// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Tx.Windows
{
    public sealed class PerfCounterFileReader : PerfCounterReader
    {
        private readonly bool _binaryLog;
        private bool _firstMove = true;

        public PerfCounterFileReader(IObserver<PerformanceSample> observer, string file)
            : base(observer)
        {
            string extension = Path.GetExtension(file);
            if (extension != null) _binaryLog = extension.ToLowerInvariant() == ".blg";

            string[] counterPaths = PdhUtils.GetCounterPaths(file);

            PdhStatus status = PdhNativeMethods.PdhOpenQuery(file, IntPtr.Zero, out _query);
            PdhUtils.CheckStatus(status, PdhStatus.PDH_CSTATUS_VALID_DATA);

            for (int i = 0; i < counterPaths.Length; i++)
            {
                AddCounter(counterPaths[i], i);
            }

            Read();
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
                    DateTime timestamp = TimeUtil.FromFileTime(time);

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

        ~PerfCounterFileReader()
        {
            Dispose();
        }
    }
}