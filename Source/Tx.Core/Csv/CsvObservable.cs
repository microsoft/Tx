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
    using System.Text;

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
            var stringBuilder = new StringBuilder();

            using(var stream = File.OpenRead(fileName))
            using (var reader = new StreamReader(stream))
            {
                ReadOnlyCollection<string> header;
                if (reader.Peek() >= 0)
                {
                    var first = this.ParseLine(reader.ReadLine(), stringBuilder).ToArray();

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
                    var items = this.ParseLine(reader.ReadLine(), stringBuilder).ToArray();

                    yield return new Record(header, items);
                }
            }
        }

        private IEnumerable<string> ParseLine(string input, StringBuilder stringBuilder)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield break;
            }

            stringBuilder.Clear();

            int index = 0;
            int escapeCount = 0;

            for (; index < input.Length; index++)
            {
                if (input[index] == '"')
                {
                    escapeCount++;
                    stringBuilder.Append('"');
                }
                else if (input[index] == this._columnSeparator)
                {
                    if ((escapeCount % 2) == 0)
                    {
                        if (escapeCount == 0)
                        {
                            yield return stringBuilder
                                .ToString();
                        }
                        else
                        {
                            yield return stringBuilder
                                .Extract('"')
                                .Replace(@"""""", @"""");
                        }

                        stringBuilder.Clear();
                        escapeCount = 0;
                    }
                    else
                    {
                        stringBuilder.Append(this._columnSeparator);
                    }
                }
                else
                {
                    stringBuilder.Append(input[index]);
                }
            }

            if (escapeCount == 0)
            {
                yield return stringBuilder
                    .ToString();
            }
            else
            {
                yield return stringBuilder
                    .Extract('"')
                    .Replace(@"""""", @"""");
            }
        }
    }

    internal static class StringBuilderExtensions
    {
        public static string Extract(this StringBuilder input, char character)
        {
            var startIndex = input.IndexOf(character);
            var lastIndex = input.LastIndexOf(character);

            var result = input.ToString(
                startIndex + 1,
                lastIndex - startIndex - 1);

            return result;
        }

        public static int LastIndexOf(this StringBuilder input, char character)
        {
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (input[i] == character)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOf(this StringBuilder input, char character)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == character)
                {
                    return i;
                }
            }

            return -1;
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
