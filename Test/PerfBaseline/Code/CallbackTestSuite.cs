// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Tx.Windows;
    
namespace PerformanceBaseline.Code
{
    class CallbackTestSuite : IPerformanceTestSuite
    {
        readonly string[] _files;
        protected Action<EtwNativeEvent> EtwCallback;
        readonly ManualResetEvent _completed;

        public CallbackTestSuite(params string[] files)
        {
            _files = files;
            _completed = new ManualResetEvent(false);
        }

        public TimeSpan  RunTestcase(MethodInfo method)
        {
            var observable = EtwObservable.FromFiles(_files);

            // the testcase should set the callback
            method.Invoke(this, new object[]{});

            Stopwatch stopwatch = Stopwatch.StartNew();
            observable.Subscribe(EtwCallback, OnCompleted);
            _completed.WaitOne();
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        void OnCompleted()
        {
            _completed.Set();
        }
    }
}
