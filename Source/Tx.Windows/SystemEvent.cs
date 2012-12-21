namespace Tx.Windows
{
    using System;
    using System.Reactive;

    // Class used to represent any ETW event, 
    // by taking the system header and no user payload data
    public class SystemHeader
    {
        public DateTime Timestamp { get; set; }
        public Guid ActivityId { get; set; }
        public Guid RelatedActivityId { get; set; }
        public Guid ProviderId { get; set; }
        public ushort EventId { get; set; }
        public byte Opcode { get; set; }
        public byte Version { get; set; }
        public string Context { get; set; }
        public uint ProcessId { get; set; }
        public uint ThreadId { get; set; }
        public byte Level { get; set; }
        public byte Channel { get; set; }
        public ushort Task { get; set; }
        public ulong Keywords { get; set; }
    }

    public class SystemEvent
    {
        public SystemHeader Header { get; set; }

        [OccurenceTime]
        public DateTime OccurenceTime
        {
            get { return Header.Timestamp; }
        }

        public override string ToString()
        {
            Func<SystemEvent, string> func = EventFormatter.GetFormatFunction(this.GetType());
            return func(this);
        }
    }

    public class SystemEventFormatted : SystemEvent
    {
        string _message;

        public string Message { get { return _message; } }
    }

    public enum EventLevel
    {
        LogAlways = 0,
        Critical = 1,
        Error = 2,
        Warning = 3,
        Informational = 4,
        Verbose = 5
    }
}
