// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

// Here we know that the event payload is simply a string, 
// So it is easier to manually write the class describing the event than writing manifest and using EtwEventTypeGen.exe

namespace OdataListener.Entities
{
    using Tx.Windows;

    [Format("Parsed request (request pointer %1, method %2) with URI %3.")]

    [ManifestEvent("{dd5ef90a-6398-47a4-ad34-4dcecdef795f}", 2, 0,
    "Parse", "win:Informational", "Microsoft-Windows-HttpService/Trace", "Flagged on all HTTP events dealing with request processing")]

    public class Parse : SystemEvent
    {
        [EventField("win:Pointer")]
        public ulong RequestObj { get; set; }

        [EventField("win:UInt32")]
        public uint HttpVerb { get; set; }

        [EventField("win:UnicodeString")]
        public string Url { get; set; }
    }
}
