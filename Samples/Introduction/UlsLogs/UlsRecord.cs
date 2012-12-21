using System;

namespace UlsLogs
{
    public class UlsRecord
    {
        DateTimeOffset _time;
        string _process;
        string _product;
        string _category;
        Guid _correlation;
        ushort _flags;
        string _message;
        string _eventId;

        public UlsRecord(string line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            string[] tokens = line.Split('\t');

            if (tokens.Length < 7)
                throw new Exception("The argument 'line' contains less than 7 tokens separated by tabs:\n" + line);

            _time = DateTimeOffset.Parse(tokens[0]);
            _process = tokens[1];
            _product = tokens[3];
            _category = tokens[4];
            _eventId = tokens[5];
            _message = tokens[7];
            _correlation = tokens[8] != "" ? new Guid(tokens[8]) : Guid.Empty;
        }

        public DateTimeOffset Time { get { return _time; } }
        public string Process { get { return _process; } }
        public string Product { get { return _product; } }
        public string Category { get { return _category; } }
        public Guid Correlation { get { return _correlation; } }
        public ushort Flags { get { return _flags; } }
        public string Message { get { return _message; } }
        public string EventId { get { return _eventId; } }
    }
}
