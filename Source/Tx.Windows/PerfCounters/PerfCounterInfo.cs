// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    internal class PerfCounterInfo : IDisposable
    {
        private readonly PdhCounterHandle _counterHandle;
        private readonly string _counterName;
        private readonly string _counterSet;
        private readonly string _instance;

        public PerfCounterInfo(string counterPath, PdhCounterHandle handle)
        {
            string[] tokens = counterPath.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
            _counterSet = tokens[1];
            _counterName = tokens[2];
            _counterHandle = handle;

            if (_counterSet.EndsWith(")"))
            {
                int index = _counterSet.LastIndexOf('(');
                _instance = _counterSet.Substring(index + 1, _counterSet.Length - index - 2);
                _counterSet = _counterSet.Substring(0, index);
            }
        }

        public string CounterSet
        {
            get { return _counterSet; }
        }

        public string CounterName
        {
            get { return _counterName; }
        }

        public string Instance
        {
            get { return _instance; }
        }

        public PdhCounterHandle Handle
        {
            get { return _counterHandle; }
        }

        public void Dispose()
        {
            _counterHandle.Dispose();
        }
    }
}