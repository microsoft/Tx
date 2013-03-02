// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Eventing.Reader;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public class EvtxObservable
    {
        public static IObservable<EventRecord> FromLog(string name)
        {
            var q = new EventLogQuery(name, PathType.LogName);
            var watcher = new EventLogWatcher(q);

            return Observable.Create<EventRecord>(o =>
                {
                    watcher.EventRecordWritten +=
                        (sender, args) =>
                            {
                                if (args.EventException != null)
                                    o.OnError(args.EventException);
                                else
                                    o.OnNext(args.EventRecord);
                            };

                    watcher.Enabled = true;

                    return watcher;
                });
        }
    }
}