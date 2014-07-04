// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public sealed class CsvObservable
    {
        private readonly char _columnSeparator;
        private readonly int _numberRecordsToSkip;

        public CsvObservable()
            : this(',', 1)
        {
        }

        public CsvObservable(char columnSeparator, int numberRecordsToSkip)
        {
            this._columnSeparator = columnSeparator;
            this._numberRecordsToSkip = numberRecordsToSkip;
        }

        public IObservable<Record> FromFiles(params string[] files)
        {
            return files.SelectMany(this.ReadRecords)
                .ToObservable(Scheduler.Default);
        }

        private IEnumerable<Record> ReadRecords(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                ReadOnlyCollection<string> header;
                if (reader.Peek() >= 0)
                {
                    var first = SplitAndTrim(reader.ReadLine(), this._columnSeparator);

                    header = new ReadOnlyCollection<string>(first);
                }
                else
                {
                    yield break;
                }

                for (var i = 0; i < this._numberRecordsToSkip && reader.Peek() >= 0; i++)
                {
                    reader.ReadLine();
                }

                while (reader.Peek() >= 0)
                {
                    yield return new Record(header, SplitAndTrim(reader.ReadLine(), this._columnSeparator));
                }
            }
        }

        public static string[] SplitAndTrim(string item, char separator)
        {
            var first = item.Split(separator);

            for (int i = 0; i < first.Length; i++)
            {
                first[i] = (first[i] ?? string.Empty).Trim();
            }

            return first;
        }
    }

    public sealed class Record
    {
        public ReadOnlyCollection<string> Header { get; set; }

        public string[] Items { get; set; }

        public Record()
        {
        }

        public Record(ReadOnlyCollection<string> header, string[] items)
        {
            this.Header = header;
            this.Items = items;
        }
    }
}
