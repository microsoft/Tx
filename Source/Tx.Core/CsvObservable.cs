// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public sealed class CsvObservable
    {
        private readonly char _columnSeparator;
        private readonly int _numberRecordsToSkip;

        public CsvObservable() : this (',', 1)
        {
        }

        public CsvObservable(char columnSeparator, int numberRecordsToSkip)
        {
            this._columnSeparator = columnSeparator;
            this._numberRecordsToSkip = numberRecordsToSkip;
        }

        public IObservable<string[]> FromFiles(params string[] files)
        {
            return files
                .SelectMany(file => ReadLines(file).Skip(this._numberRecordsToSkip))
                .Select(record => record.Split(this._columnSeparator))
                .ToObservable(Scheduler.Default);
        }

        private static IEnumerable<string> ReadLines(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                while (reader.Peek() >= 0)
                {
                    yield return reader.ReadLine();
                }
            }
        }
    }
}
