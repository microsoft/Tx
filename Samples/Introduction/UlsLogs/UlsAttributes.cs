using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UlsLogs
{
    [AttributeUsage(AttributeTargets.Class)]
    class UlsEventAttribute : Attribute
    {
        readonly string _eventId;
        readonly string _regex;

        public UlsEventAttribute(string eventId, string regex)
        {
            _eventId = eventId;
            _regex = regex;
        }

        public string EventId { get { return _eventId; } }
        public string RegEx { get { return _regex; } }
    }
}
