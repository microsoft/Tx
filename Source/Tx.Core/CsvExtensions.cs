// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.IO;

namespace System.Reactive
{
    public static class CsvExtensions
    {
        public static IDisposable ToCsvFile<T>(this IObservable<T> source, string filePath)
        {
            return source.Subscribe(new TextFileWriter<T>(", ", filePath));
        }

        public static IDisposable ToTsvFile<T>(this IObservable<T> source, string filePath)
        {
            return source.Subscribe(new TextFileWriter<T>("\t", filePath));
        }

        class TextFileWriter<T> : IObserver<T>, IDisposable
        {
            private string _separator;
            private StreamWriter _writer;
            private bool _wroteHeader = false;

            public TextFileWriter(string separator, string filePath)
            {
                _writer = File.CreateText(filePath);
            }

            public void OnCompleted()
            {
                _writer.Close();
            }

            public void OnError(Exception error)
            {
                throw error;
            }

            public void OnNext(T value)
            {
                if (!_wroteHeader)
                {
                    WriteHeader(value);
                    _wroteHeader = true;
                }

                bool isFirst = true;
                foreach (var p in typeof(T).GetProperties())
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        _writer.Write(_writer);

                    var propValue = p.GetValue(value);
                    IDictionary dictionary = propValue as IDictionary;

                    if (dictionary == null)
                        _writer.Write(propValue.ToString());
                    else
                        WriteValuesAsRow(dictionary);
                }

                _writer.WriteLine();
            }

            public void Dispose()
            {
                if (_writer != null)
                {
                    _writer.Dispose();
                    _writer = null;
                }
            }

            void WriteHeader(T firstValue)
            {
                bool isFirst = true;
                foreach (var p in typeof(T).GetProperties())
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        _writer.Write(_separator);

                    var propValue = p.GetValue(firstValue);
                    IDictionary dictionary = propValue as IDictionary;

                    if (dictionary == null)
                        _writer.Write(p.Name);
                    else
                        WriteKeysAsColumnTitles(dictionary);
                }

                _writer.WriteLine();
            }

            void WriteKeysAsColumnTitles(IDictionary dictionary)
            {
                bool isFirst = true;

                foreach (var key in dictionary.Keys)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        _writer.Write(_separator);    
                
                    _writer.Write(key);
                }
            }

            void WriteValuesAsRow(IDictionary dictionary)
            {
                bool isFirst = true;

                foreach (var value in dictionary.Values)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        _writer.Write(_separator);

                    _writer.Write(value.ToString());
                }
            }
        }
    }
}
