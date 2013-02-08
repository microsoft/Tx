// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

// Here we know that the event payload is simply a string, 
// So it is easier to manually write the class describing the event than writing manifest and using EtwEventTypeGen.exe

namespace Tx.Windows.BounceMessages
{
    [ManifestEvent("{8400115e-3a7a-4fb0-95ca-6121397f7c4a}", 0, 0)]
    public class TracedEvent : SystemEvent
    {
        [EventField("win:UnicodeString")]
        public string Message { get; set; }
    }
}
