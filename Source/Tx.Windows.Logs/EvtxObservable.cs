// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;

using System.Reactive.Linq;
using System.Security;
using System.Threading;

namespace Tx.Windows
{
    public class EvtxObservable
    {
        /// <summary>
        /// Creates an observable on a folder and can watch new files dropped.
        /// </summary>
        /// <param name="path">Path to the folder.</param>
        /// <param name="watchNewFiles">Watch new files dropped in the folder.</param>
        /// <returns></returns>
        public static IObservable<EventLogRecord> FromFolder(string path, bool watchNewFiles)
        {
            var files = Directory.EnumerateFiles(path).ToArray();
            IObservable<EventLogRecord> observable = FromFiles(files);

            if (watchNewFiles)
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(path);
                fileSystemWatcher.EnableRaisingEvents = true;

                IObservable<EventLogRecord> observable2 = Observable.Create<EventLogRecord>(observer =>
                {
                    fileSystemWatcher.Created += (sender, e) =>
                    {
                        // This sleep ensures that the file is readable after creation.
                        // Without this, an exception is encountered
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        var events = EvtxEnumerable.FromFiles(e.FullPath);
                        foreach (var item in events)
                        {
                            observer.OnNext(item);
                        }
                    };

                    return fileSystemWatcher;
                });

                observable = observable.Concat(observable2);
            }

            return observable;
        }

        public static IObservable<EventLogRecord> FromFiles(params string[] files)
        {
            return EvtxEnumerable.FromFiles(files).ToObservable();
        }

        public static IObservable<EventLogRecord> TryGetObservable(string[] files, out IDictionary<string, Exception> fileReadExceptions)
        {
            fileReadExceptions = null;

            List<EventLogRecord> events = new List<EventLogRecord>();
            foreach (string file in files)
            {
                try
                {
                    var result = EvtxEnumerable.FromFiles(file);
                    events.AddRange(result);
                }
                catch (EventLogException logEx)
                {
                    if (fileReadExceptions != null)
                    {
                        fileReadExceptions.Add(file, logEx);
                    }
                    else
                    {
                        fileReadExceptions = new Dictionary<string, Exception>();
                        fileReadExceptions.Add(file, logEx);
                    }
                }
            }

            return events.ToObservable();
        }

        public static IObservable<EventLogRecord> FromLog(string name, string query = null, bool readExisting = true, EventBookmark bookmark = null)
        {
            var q = string.IsNullOrEmpty(query) ? new EventLogQuery(name, PathType.LogName) : new EventLogQuery(name, PathType.LogName, query);
            return CreateEventRecordObservable(q, bookmark, readExisting);
        }

        public static IObservable<EventLogRecord> FromRemoteServerLog(string logName, string server, bool readExisting = true)
        {
            EventLogSession session = new EventLogSession(server);

            var q = new EventLogQuery(logName, PathType.LogName)
            {
                Session = session
            };

            return CreateEventRecordObservable(q, null, readExisting);
        }

        public static IObservable<EventLogRecord> FromRemoteServerLog(string logName, string server, string userDomain, string username, SecureString password, bool readExisting = true)
        {
            EventLogSession session = new EventLogSession(server, userDomain, username, password, SessionAuthentication.Default);

            var q = new EventLogQuery(logName, PathType.LogName)
            {
                Session = session
            };

            return CreateEventRecordObservable(q, null, readExisting);
        }

        private static IObservable<EventLogRecord> CreateEventRecordObservable(
            EventLogQuery query, 
            EventBookmark bookmark, 
            bool readExistingEvents)
        {
            var watcher = new EventLogWatcher(query, bookmark, readExistingEvents);

            IObservable<EventLogRecord> observable = Observable.Create<EventLogRecord>(o =>
            {
                watcher.EventRecordWritten +=
                    (sender, args) =>
                    {
                        if (args.EventException != null)
                        {
                            o.OnError(args.EventException);
                        }
                        else
                        {
                            o.OnNext(args.EventRecord as EventLogRecord);
                        }
                    };

                watcher.Enabled = true;
                return watcher;
            });

            return observable;
        }
    }
}