// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    internal class PerfCounterInfo : IDisposable
    {
        private readonly PdhCounterHandle _counterHandle;
        private readonly string _machine;
        private readonly string _counterName;
        private readonly string _counterSet;
        private readonly string _instance;
        private readonly int _index;  // this is the sequence # in which the counter was added

        public PerfCounterInfo(string counterPath, PdhCounterHandle handle, int index)
        {
            _index = index;

            string[] tokens = counterPath.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                _machine = Environment.MachineName;
                _counterSet = tokens[0];
                _counterName = tokens[1];
            }
            else
            {
                _machine = tokens[0];
                _counterSet = tokens[1];
                _counterName = tokens[2];
                _counterHandle = handle;
            }

            if (_counterSet.EndsWith(")"))
            {
                int openIndex = _counterSet.LastIndexOf('(');
                _instance = _counterSet.Substring(openIndex + 1, _counterSet.Length - openIndex - 2);
                _counterSet = _counterSet.Substring(0, openIndex);
            }

            _counterHandle = handle;
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