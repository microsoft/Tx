using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;


namespace System.Reactive
{
    public static class EvtxEnumerable
    {
        public static IEnumerable<EventRecord> ReadLog(string logName)
        {
            using (var reader = new EventLogReader(logName, PathType.LogName))
            {
                for (;;)
                {
                    EventRecord record = reader.ReadEvent();
                    if (record == null)
                        yield break;

                    yield return record;
                }
            }
        }
    }
}