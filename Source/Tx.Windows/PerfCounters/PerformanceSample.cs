// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    public class PerformanceSample
    {
        private readonly PerfCounterInfo _counterInfo;
        private readonly DateTime _timestamp;
        private readonly double _value;

        internal PerformanceSample(PerfCounterInfo counterInfo, DateTime timestamp, double value)
        {
            _counterInfo = counterInfo;
            _timestamp = timestamp;
            _value = value;
        }

        public PerformanceSample(PerformanceSample other)
        {
            _counterInfo = other._counterInfo;
            _timestamp = other._timestamp;
            _value = other._value;
        }

        public string CounterSet
        {
            get { return _counterInfo.CounterSet; }
        }

        public string CounterName
        {
            get { return _counterInfo.CounterName; }
        }

        public string Instance
        {
            get { return _counterInfo.Instance; }
        }

        public string Machine
        {
            get { return _counterInfo.Machine; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public double Value
        {
            get { return _value; }
        }
    }
}