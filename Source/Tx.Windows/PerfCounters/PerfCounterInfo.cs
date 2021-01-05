// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Tx.Windows
{
    internal class PerfCounterInfo : IDisposable
    {
        private readonly PdhCounterHandle _counterHandle;
        private readonly string _machine;
        private readonly string _counterName;
        private readonly string _counterPath;
        private readonly string _counterSet;
        private readonly string _instance;
        private readonly int _index;  // this is the sequence # in which the counter was added

        public PerfCounterInfo(string counterPath, PdhCounterHandle handle, int index)
        {
            _counterPath = counterPath;
            _index = index;
            string counterPattern = @"(\\\\){0,1}(?<machine>.+?){0,1}\\(?<object>.+?)(?<instance>\(.*?\)){0,1}\\(?<counter>.+)";

            Match counterInfo = Regex.Match(counterPath, counterPattern);
            _machine = Environment.MachineName;
            
            if(counterInfo.Groups["machine"]?.Value != null)
            {
                _machine = counterInfo.Groups["machine"].Value;
            }

            _counterHandle = handle;
            _counterName = counterInfo.Groups["counter"].Value;
            _counterSet = counterInfo.Groups["object"].Value;
            _instance = Regex.Replace(counterInfo.Groups["instance"].Value, @"^\(|\)$", "");
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
            get { return _counterPath; }
        }

        public string Instance
        {
            get { return _instance; }
        }

        public PdhCounterHandle Handle
        {
            get { return _counterHandle; }
        }

        public string Machine
        {
            get { return _machine; }
        }

        public int Index
        {
            get { return _index; }
        }

        public void Dispose()
        {
            _counterHandle.Dispose();
        }
    }
}