using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;

namespace UlsLogs
{
    class UlsEnumerable
    {
        public static IEnumerable<UlsRecord> FromFiles(params string[] ulsFiles)
        {
            if (ulsFiles.Length == 1)
                return FromFile(ulsFiles[0]);

            var inputs = from file in ulsFiles select FromFile(file).GetEnumerator();
            return new PullMergeSort<UlsRecord>(e => e.Time.DateTime, inputs);
        }

        static IEnumerable<UlsRecord> FromFile(string ulsFile)
        {
            using (TextReader reader = File.OpenText(ulsFile))
            {
                int lineNumber = 0; // for debugging
                for (; ; )
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        yield break;

                    lineNumber++;
                    yield return new UlsRecord(line);
                }
            }
        }
    }
}
