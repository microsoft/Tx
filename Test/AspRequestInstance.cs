namespace Tests.Tx
{
    using System;

    public class AspRequestInstance
    {
        public Guid RequestId { get; set; }
        public DateTime RecvReq { get; set; }
        public DateTime Start { get; set; }
        public DateTime StartHandler { get; set; }
        public DateTime HttpHandlerEnter { get; set; }
        public DateTime HttpHanlerLeave { get; set; }
        public DateTime EndHandler { get; set; }
        public DateTime End { get; set; }

        public AspRequestInstance Merge(AspRequestInstance other)
        {
            if (other.RecvReq != default(DateTime))
                RecvReq = other.RecvReq;

            if (other.Start != default(DateTime))
                Start = other.Start;

            if (other.StartHandler != default(DateTime))
                StartHandler = other.StartHandler;

            if (other.HttpHandlerEnter != default(DateTime))
                HttpHandlerEnter = other.HttpHandlerEnter;

            if (other.HttpHanlerLeave != default(DateTime))
                HttpHanlerLeave = other.HttpHanlerLeave;

            if (other.EndHandler != default(DateTime))
                EndHandler = other.EndHandler;

            if (other.End != default(DateTime))
                End = other.End;

            return this;
        }

        public bool IsCompleted
        {
            get { return End != default(DateTime); }
        }
    }
}
