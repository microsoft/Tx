// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PerformanceCounterAttribute : Attribute
    {
        private readonly string _counterName;
        private readonly string _counterSet;

        public PerformanceCounterAttribute(string counterSet, string counterName)
        {
            _counterSet = counterSet;
            _counterName = counterName;
        }

        public string CounterSet
        {
            get { return _counterSet; }
        }

        public string CounterName
        {
            get { return _counterName; }
        }

        public string CounterPath
        {
            get { return @"\\GEORGIS5\" + CounterSet + @"(*)\" + CounterName; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(_counterSet);
            sb.AppendLine(_counterName);

            return sb.ToString();
        }
    }
}