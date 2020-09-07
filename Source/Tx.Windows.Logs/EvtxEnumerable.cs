// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive;
using System.Xml.Linq;

namespace Tx.Windows
{
    public static class EvtxEnumerable
    {
        public static IEnumerable<EventLogRecord> FromFiles(params string[] logfiles)
        {
            if (logfiles.Length == 1)
                return FromFile(logfiles[0]);

            IEnumerable<IEnumerator<EventLogRecord>> inputs = from file in logfiles select FromFile(file).GetEnumerator();

            return new PullMergeSort<EventLogRecord>(e => e.TimeCreated.Value.ToUniversalTime(), inputs);
        }

        /// <summary>
        /// Creates multiple log readers as specified in the XML
        /// which has the same format as Windows Event Collection
        /// Note that logs that don't exist on the local machine are skipped
        /// </summary>
        /// <param name="wecXml">XML configuration about XPath expressions over multiple logs</param>
        /// <returns></returns>
        public static IEnumerable<EventLogRecord> FromWecXml(string wecXml)
        {
            if (string.IsNullOrEmpty(wecXml))
            {
                throw new ArgumentNullException(nameof(wecXml));
            }

            var readers = new List<IEnumerable<EventLogRecord>>();
            XDocument xd = XDocument.Parse(wecXml);
            foreach (var query in xd.Root.Elements("Query"))
            {
                string logName = query.Attribute("Path").Value;

                foreach (var select in query.Elements("Select"))
                {
                    if (EventLog.Exists(logName))
                    {
                        readers.Add(FromLogQuery(logName, select.Value, null));
                    }
                }
            }

            return new PullMergeSort<EventLogRecord>(
                r => r.TimeCreated.Value,
                readers.Select(r => r.GetEnumerator()));
        }

        public static IEnumerable<EventLogRecord> FromLogQuery(string logName, string xpathQuery, EventBookmark bookmark)
        {
            long eventCount = 0; // for debugging

            EventLogQuery query = new EventLogQuery(logName, PathType.LogName, xpathQuery);
            using (var reader = new EventLogReader(query, bookmark))
            {
                for (; ; )
                {
                    if (!(reader.ReadEvent() is EventLogRecord record))
                    {
                        yield break;
                    }

                    eventCount++;
                    yield return record;
                }
            }
        }

        private static IEnumerable<EventLogRecord> FromFile(string logFile)
        {
            long eventCount = 0; // for debugging
            using (var reader = new EventLogReader(logFile, PathType.FilePath))
            {
                for (; ; )
                {
                    if (!(reader.ReadEvent() is EventLogRecord record))
                    {
                        yield break;
                    }

                    eventCount++;
                    yield return record;
                }
            }
        }
    }
}