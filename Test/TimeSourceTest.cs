using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Tests.Tx
{
    [TestClass]
    public class TimeSourceTest : ReactiveTest
    {
        static Recorded<Notification<long>>[] s_testData = new Recorded<Notification<long>>[] 
        {
            OnNext(101),
            OnNext(102),
            OnNext(106),    // Events     1 2       6   8 9           15
            OnNext(108),    //          | . . . . + . . . . | . . . . + . . . . | . -->
            OnNext(109),    //         100       105       110       115       120 Time
            OnNext(115),
            OnCompleted<long>(120)
        };

        [TestMethod]
        public void TimeSource100()
        {
            TestScheduler scheduler = new TestScheduler();
            var xs = scheduler.CreateHotObservable(s_testData);

            var timeSource = new TimeSource<long>(xs, x => new DateTimeOffset(x, TimeSpan.Zero));
            timeSource.StartTime = new DateTimeOffset(100, TimeSpan.Zero);

            var counts = from window in timeSource.Window(TimeSpan.FromTicks(5), timeSource.Scheduler)
                         from Count in window.Count()
                         select Count;

            var list = new List<Timestamped<int>>();
            counts
                .Timestamp(timeSource.Scheduler)
                .Subscribe(ts=>list.Add(ts));

            timeSource.Connect();
            scheduler.AdvanceTo(120);

            Assert.AreEqual(4, list.Count);
            list.AssertEqual(
                Result(105, 2),
                Result(110, 3),
                Result(115, 0),
                Result(115, 1));
        }

        [TestMethod]
        public void TimeSource101()
        {
            TestScheduler scheduler = new TestScheduler();
            var xs = scheduler.CreateHotObservable(s_testData);

            var timeSource = new TimeSource<long>(xs, x => new DateTimeOffset(x, TimeSpan.Zero));
            timeSource.StartTime = new DateTimeOffset(101, TimeSpan.Zero);

            var counts = from window in timeSource.Window(TimeSpan.FromTicks(5), timeSource.Scheduler)
                         from Count in window.Count()
                         select Count;

            var list = new List<Timestamped<int>>();
            counts
                .Timestamp(timeSource.Scheduler)
                .Subscribe(ts => list.Add(ts));

            timeSource.Connect();
            scheduler.AdvanceTo(120);

            Assert.AreEqual(3, list.Count);
            list.AssertEqual(
                Result(106, 2),
                Result(111, 3),
                Result(115, 1));
        }

        [TestMethod]
        public void TimeAutoStart()
        {
            TestScheduler scheduler = new TestScheduler();
            var xs = scheduler.CreateHotObservable(s_testData);

            var timeSource = new TimeSource<long>(xs, x => new DateTimeOffset(x, TimeSpan.Zero));
            // Note: no start time specified, result should be the same as 101

            var counts = from window in timeSource.Window(TimeSpan.FromTicks(5), timeSource.Scheduler)
                         from Count in window.Count()
                         select Count;

            var list = new List<Timestamped<int>>();
            counts
                .Timestamp(timeSource.Scheduler)
                .Subscribe(ts => list.Add(ts));

            timeSource.Connect();
            scheduler.AdvanceTo(120);

            Assert.AreEqual(3, list.Count);
            list.AssertEqual(
                Result(106, 2),
                Result(111, 3),
                Result(115, 1));
        }

        public static Recorded<Notification<long>> OnNext(long ticks)
        {
            return OnNext(ticks, ticks);
        }

        public static Timestamped<int> Result(long ticks, int value)
        {
            DateTimeOffset timestamp = new DateTimeOffset(ticks, TimeSpan.Zero);
            return new Timestamped<int>(value, timestamp);
        }
    }
}
