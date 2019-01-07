// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive;

namespace Tx.Windows
{
    public static class EvtxEnumerable
    {
        public static IEnumerable<EventRecord> FromFiles(params string[] logfiles)
        {
            if (logfiles.Length == 1)
                return FromFile(logfiles[0]);

            IEnumerable<IEnumerator<EventRecord>> inputs = from file in logfiles select FromFile(file).GetEnumerator();

            return new PullMergeSort<EventRecord>(e => e.TimeCreated.Value.ToUniversalTime(), inputs);
        }

        private static IEnumerable<EventRecord> FromFile(string logFile)
        {
            long eventCount = 0; // for debugging
            using (var reader = new EventLogReader(logFile, PathType.FilePath))
            {
                for (;;)
                {
                    EventRecord record = reader.ReadEvent();
                    if (record == null)
                        yield break;

                    eventCount++;
                    yield return record;
                }
            }
        }
    }
}