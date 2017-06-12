using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Tx.LinqPad
{
    [EventSource(Name = "Microsoft-Tx-LinqpadDriver")]
    public sealed class TxEventSource : EventSource
    {
        public static readonly TxEventSource Log = new TxEventSource();

        public class Keywords
        {
            public const EventKeywords Debug = ((EventKeywords)(1));

            public const EventKeywords Perf = ((EventKeywords)(2));

            public const EventKeywords Telemetry = ((EventKeywords)(4));

            public const EventKeywords Auditing = ((EventKeywords)(8));
        }

        [Event(1, Level = EventLevel.Error, Keywords = Keywords.Debug, Message = "File: {0} Method: {1}, Exception: {2}")]
        public void TraceError(string exception, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, fileName, methodName, exception);
            }
        }
    }
}
