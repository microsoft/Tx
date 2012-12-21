namespace WcfInterception
{
    using System;
    using System.Diagnostics.Eventing;

    // To quote Rafael: We are not in the business of telling people how to trace
    // TraceInsight should work regardless of the specifics, given people have adequate parsers and queries

    // This example illustrates firing good ETW events with the .Net API that is shipping today

    // This has two disadvantages:
    //  1) complex API
    //  2) manually written manifest
    //
    //  Much better story is the EventSource that ships in .Net 4.5
    //  See http://toolbox/EventSource

    public static class WcfEvents
    {
        static EventProvider _provider;
        static EventDescriptor _startRequest;
        static EventDescriptor _endRequest;

        static WcfEvents()
        {
            _provider = new EventProvider(new Guid("83093276-1f35-45a2-8b19-6964cc85c70f"));
            _startRequest = new EventDescriptor(1, 0, 0, 4, 0, 0, 1);
            _endRequest = new EventDescriptor(2, 0, 0, 4, 0, 0, 1);
        }

        public static void StartRequest(string operationName, string requestMessageId)
        {
            _provider.WriteEvent(ref _startRequest, operationName, requestMessageId);
            
        }

        public static void EndRequest(string requestMessageId)
        {
            _provider.WriteEvent(ref _endRequest, requestMessageId);
        }
    }
}
