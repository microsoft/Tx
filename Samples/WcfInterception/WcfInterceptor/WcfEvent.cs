using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;

namespace WcfInterception
{
    [DataContract]
    public class MethodCallDuration
    {
        [DataMember]
        public TimeSpan duration;
        [DataMember]
        public string methodName;

        public MethodCallDuration(string methodName, DateTime from, DateTime to)
        {
            this.methodName = methodName;
            duration = to.Subtract(from);
        }

        public override string ToString()
        {
            return "MethodCallDuration: methodName=" + methodName + "   duration=" + duration;
        }
    }

    [DataContract]
    public class WcfEvent
    {
        string correlationId;
        string operationName;

        public WcfEvent(string correlationId, string operationName)
        {
            this.correlationId = correlationId;
            this.operationName = operationName;
        }

        [DataMember]
        public string CorrelationId
        {
            get { return correlationId; }
            set { correlationId = value; }
        }

        [DataMember]
        public string OperationName
        {
            get { return operationName; }
            set { operationName = value; }
        }

        public override string ToString()
        {
            return "WcfEvent: correlationId=" + correlationId + "   operationName=" + operationName;
        }
    }
}
