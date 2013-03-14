namespace Test.TraceInsight.PerformanceBaseline
{
    using System.Reactive.Linq;
    using Microsoft.TraceInsight;
    using Microsoft.TraceInsight.Etw;

    [PerformanceTestSuite("MSNT_SystemTrace", "Rx")]
    class Rx_CSwitch : RxTestSuite
    {
        public Rx_CSwitch()
            : base(@"..\..\..\..\..\Traces\MSNT_SystemTrace.etl")
        { }

        [PerformanceTestCase("Context Switch")]
        public void CSwitch()
        {
            var cswitchEvents = _playback.GetStream<MSNT_SystemTrace.CSwitch>();
            var start = from cs in cswitchEvents
                        select
                            new
                            {
                                Timestamp = cs.Header.Timestamp,
                                ThreadId = cs.NewThreadId
                            };

            var end = from cs in cswitchEvents
                      select new
                      {
                          Timestamp = cs.Header.Timestamp,
                          ThreadId = cs.OldThreadId
                      };

            var active = from s in start
                         from e in end.Where(e => e.ThreadId == s.ThreadId).Take(1)
                         select new
                         {
                             ThreadId = s.ThreadId,
                             Duration = e.Timestamp - s.Timestamp
                         };

            var summary = from a in active
                          group a by new
                          {
                              ThreadId = a.ThreadId
                          }
                        into eachGroup
                        from c in eachGroup.Aggregate((long)0, (a,e)=>a+e.Duration.Ticks)
                        select new 
                        {
                            ThreadId = eachGroup.Key.ThreadId,
                            TotalTicks = c 
                        };

            RegisterForValidation(summary, 160);
        }
    }
}

