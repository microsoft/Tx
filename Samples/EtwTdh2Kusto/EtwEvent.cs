using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Etw2Kusto
{
    /// <summary>
    /// Class that is intended for Kusto upload
    /// This reverses the schema choices about how ETW is represented in Tx:
    /// -  The system data (Header) become top level properties
    /// -  The top-level properties become EventData (dynamic in Kusto)
    /// 
    /// Note that 
    /// -  In Kusto, this is useful to allow indexing of the system fields
    /// -  In Tx, there are no indexes - Rx queries work on replay-able stream 
    ///    customers of Tx rarely used the system fields (e.g. event ID) explicitly
    /// </summary>
    public class EtwEvent
    {
        public string Provider;
        public int EventId;
        public int Version;
        public string Level;
        public string Task;
        public string Opcode;
        public DateTime TimeCreated;
        public int ProcessId;
        public int ThreadId;
        public string Channel;
        public dynamic EventData;
        public Guid ActivityId;
        public Guid RealtedActivityId;
        public string Message;

        public EtwEvent(IDictionary<string, object> evt)
        {
            Provider = (string)evt["Provider"];
            EventId = (ushort)evt["EventId"];
            Version = (byte)evt["Version"];
            Level = (string)evt["Level"];
            Task = (string)evt["Task"];
            Opcode = (string)evt["Opcode"];
            TimeCreated = (DateTime)evt["TimeCreated"];
            ProcessId = (int)(uint)evt["ProcessId"];
            ThreadId = (int)(uint)evt["ThreadId"];
            Channel = (string)evt["Channel"];

            string json = JsonConvert.SerializeObject(evt["EventData"], Formatting.Indented);
            EventData = json;

            ActivityId = (Guid)evt["ActivityId"];

            if (evt.ContainsKey("Message"))
            {
                Message = (string)evt["Message"];
            }
        }
    }
}

