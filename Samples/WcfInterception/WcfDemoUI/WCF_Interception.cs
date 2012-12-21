namespace Microsoft.Etw.WcfInterception
{
    using System;

    [ManifestEvent("83093276-1f35-45a2-8b19-6964cc85c70f", 1, 0)]
    public class StartRequest : SystemEvent
    {
        [EventField("win:UnicodeString", "")]
        public string operationName { get; set; }

        [EventField("win:UnicodeString", "")]
        public string requestMessageId { get; set; }
    }

    [ManifestEvent("83093276-1f35-45a2-8b19-6964cc85c70f", 2, 0)]
    public class EndRequest : SystemEvent
    {
        [EventField("win:UnicodeString", "")]
        public string requestMessageId { get; set; }
    }
}
