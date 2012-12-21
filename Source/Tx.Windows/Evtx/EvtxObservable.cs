using System;
using System.Diagnostics.Eventing.Reader;
using System.Reactive.Linq;

namespace Tx.Windows
{
    public class EvtxObservable
    {
        public static IObservable<EventRecord> FromLog(string name)
        {
            EventLogQuery q = new EventLogQuery(name, PathType.LogName);
            EventLogWatcher watcher = new EventLogWatcher(q);

            return Observable.Create<EventRecord>(o =>
                {
                    watcher.EventRecordWritten +=
                    new EventHandler<EventRecordWrittenEventArgs>((sender, args) =>
                        {
                            if (args.EventException != null)
                                o.OnError(args.EventException);
                            else
                                o.OnNext(args.EventRecord);
                        });

                    watcher.Enabled = true;  

                    return watcher;
                });
        }
    }
}
