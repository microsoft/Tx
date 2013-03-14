// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using Tx.Windows;

namespace PerformanceBaseline.Rx
{
    public abstract class RxTestSuite : IPerformanceTestSuite
    {
        protected Playback Playback;
        readonly List<ValidationRecord> _toValidate; // set breakpoint after scope.Run() and manualy valudate this

        protected RxTestSuite(params string[] files)
        {
            Playback = new Playback();
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();

                switch (ext)
                {
                    case ".etl":
                        Playback.AddEtlFiles(file);
                        break;

                    case ".evtx":
                        Playback.AddLogFiles(file);
                        break;

                    default:
                        throw new Exception("Unknown file type " + ext);
                }
            }

            _toValidate = new List<ValidationRecord>();
        }

        public virtual TimeSpan RunTestcase(MethodInfo method)
        {
            method.Invoke(this, new object[] { });

            Playback.Run();
            ValidateAllOutputs();

            return Playback.ExecutionDuration;
        }

        protected void RegisterForValidation<T>(IObservable<T> stream, int expectedCount)
        {
            IEnumerable collection = Playback.BufferOutput(stream);
            _toValidate.Add(new ValidationRecord
            {
                Enumerable = collection,
                ExpectedCount = expectedCount
            });
        }

        void ValidateAllOutputs()
        {
            foreach (ValidationRecord v in _toValidate)
            {
                int actualCount = v.Enumerable.OfType<object>().Count();
                if (actualCount != v.ExpectedCount)
                {
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Expected count {0}, actual count {1}", v.ExpectedCount, actualCount);
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        struct ValidationRecord
        {
            public IEnumerable Enumerable;
            public int ExpectedCount;
        }
    }
}
