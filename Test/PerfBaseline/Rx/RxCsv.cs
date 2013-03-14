// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive.Linq;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_CsvFs_Diagnostic;

namespace PerformanceBaseline.Rx
{
    [PerformanceTestSuite("Cluster Shared Volume", "Rx")]
    class CSVRx : RxTestSuite
    {
        public CSVRx()
            : base(@"CSV.etl")
        { }

        [PerformanceTestCase("EventCount")]
        public void EventCount()
        {
            var all = Playback.GetObservable<SystemEvent>();
            var count = all.Count();

            RegisterForValidation(count, 1);
        }

        [PerformanceTestCase("Event Type Statistics")]
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

            RegisterForValidation(statistics, 11);
        }

        [PerformanceTestCase("Slow Activities")]
        public void SlowActivities()
        {
            var start = Playback.GetObservable<IoStart>();
            var end = Playback.GetObservable<IoCompleted>();

            var activities = from s in start
                      from e in end.Where(e => e.Header.ActivityId == s.Header.ActivityId).Take(1)
                      select new
                      {
                          Id = s.Header.ActivityId,
                          MajorFunction = s.MajorFunction == 0 ? "MJ_CREATE" : "MJ_CLENUP",
                          MinorFunction = s.MinorFunction.ToString("x"),
                          IrpFlags = s.IrpFlags.ToString("x"),
                          Status = e.Status == 0xc0000034 ? "STATUS_OBJECT_NAME_NOT_FOUND" : e.Status.ToString("x"),
                          Duration = e.Header.Timestamp - s.Header.Timestamp
                      };

            var slow = from a in activities
                       where a.Duration.TotalMilliseconds > 18
                       select a;

            RegisterForValidation(slow, 6);
        }

        [PerformanceTestCase("Aggregate Duration")]
        public void AggregateDuration()
        {
            var start = Playback.GetObservable<IoStart>();
            var end = Playback.GetObservable<IoCompleted>();

            var activities = from s in start
                      from e in end.Where(e => e.Header.ActivityId == s.Header.ActivityId).Take(1)
                      select new
                      {
                          Id = s.Header.ActivityId,
                          MajorFunction = s.MajorFunction == 0 ? "MJ_CREATE" : "MJ_CLENUP",
                          MinorFunction = s.MinorFunction.ToString("x"),
                          IrpFlags = s.IrpFlags.ToString("x"),
                          Status = e.Status == 0xc0000034 ? "STATUS_OBJECT_NAME_NOT_FOUND" : e.Status.ToString("x"),
                          Duration = e.Header.Timestamp - s.Header.Timestamp
                      };

            var summary = from a in activities
                       group a by new
                       {
                           Milliseconds = (int)a.Duration.TotalMilliseconds,
                           a.MajorFunction,
                           a.MinorFunction,
                           a.IrpFlags,
                           a.Status
                       }
                           into groups
                           from c in groups.Count()
                           select new
                           {
                               groups.Key.MajorFunction,
                               groups.Key.MinorFunction,
                               groups.Key.IrpFlags,
                               groups.Key.Status,
                               groups.Key.Milliseconds,
                               Count = c
                           };

                RegisterForValidation(summary, 17);
        }
    }
}
