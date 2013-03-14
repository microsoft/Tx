// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive.Linq;
using Tx.Windows;
    
namespace PerformanceBaseline.Rx
{
    [PerformanceTestSuite("HTTP_Server", "Rx")]
    class RxHttp : RxTestSuite
    {
        public RxHttp()
            : base("HTTP_Server.etl", "HTTP_Server.evtx")
        {
        }

        [PerformanceTestCase("EventTypeStatistics")]
        public void EventTypeStatistics()
        {
            var all = Playback.GetObservable<SystemEvent>();
            var statistics = from e in all
                             group e by new { e.Header.ProviderId, e.Header.EventId, e.Header.Opcode, e.Header.Version }
                                 into g
                                 from c in g.Count()
                                 select new
                                 {
                                     g.Key,
                                     Count = c,
                                 };

            RegisterForValidation(statistics, 12); 
        }
    }
}
