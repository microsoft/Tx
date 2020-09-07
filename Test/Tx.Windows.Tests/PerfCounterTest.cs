using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tx.Windows.Tests
{
    [TestClass]
    public class PerfCounterTest
    {
        string BlgFileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"BasicPerfCounters.blg");
            }
        }

        [TestMethod]
        public void BlgCount()
        {
            var observable = PerfCounterObservable.FromFile(BlgFileName);

            int count = observable.Count()
                .Wait();

            Assert.AreEqual(6000, count);
        }

        [TestMethod]
        public void BlgStatistics()
        {
            var observable = PerfCounterObservable.FromFile(BlgFileName);

            int count = observable
                .Where(p => p.CounterSet == "PhysicalDisk")
                .Count()
                .Wait();

            Assert.AreEqual(3000, count);
        }

        [TestMethod]
        public void BlgPivot()
        {
            // this query pivots the counters as one row per instance, containing all the counters
            // so we get 3000/5 = 600 rows, each with 5 counters
 
            var observable = PerfCounterObservable.FromFile(BlgFileName);

            var grouped = from a in observable where a.CounterSet == "PhysicalDisk"
                          group a by new { a.Machine, a.Instance, a.Timestamp } into groups
                          from g in groups.ToArray()
                          select new
                          {
                              groups.Key.Machine,
                              groups.Key.Instance,
                              groups.Key.Timestamp,
                              Counters = g
                          };

            var all = grouped.ToEnumerable().ToArray();

            Assert.AreEqual(600, all.Length);
        }

        [TestMethod]
        public void BlgPivotTwoToFiles()
        {
            // this query pivots the counters into separate collections for Processor and PhysicalDisk

            Playback playback = new Playback();
            playback.AddPerfCounterTraces(BlgFileName);

            var all = playback.GetObservable<PerformanceSample>();

            IDisposable processor = PivotToInstanceSnapshots(playback, "Processor", "Processor.csv");
            IDisposable disk = PivotToInstanceSnapshots(playback, "PhysicalDisk", "PhysicalDisk.csv");

            playback.Run();

            processor.Dispose();
            disk.Dispose();

            Assert.IsTrue(File.Exists("Processor.csv"));
            Assert.IsTrue(File.Exists("PhysicalDisk.csv"));
        }

        IDisposable PivotToInstanceSnapshots(Playback playback, string counterSet, string filePath)
        {
            var all = playback.GetObservable<PerformanceSample>();

            var instanceSnapshots = from a in all
                                    where a.CounterSet == counterSet
                                    group a by new { a.Machine, a.Instance, a.Timestamp } into groups
                                    from g in groups.ToArray()
                                    select new 
                                    {
                                        groups.Key.Machine,
                                        groups.Key.Instance,
                                        groups.Key.Timestamp,
                                        Counters = g.ToDictionary(
                                            ps=>ps.CounterName,
                                            ps=>ps.Value)
                                    };

            return instanceSnapshots.ToCsvFile(filePath);
        }

        [TestMethod]
        public void BlgPivotTwo()
        {
            // this query pivots the counters into separate collections for Processor and PhysicalDisk

            Playback playback = new Playback();
            playback.AddPerfCounterTraces(BlgFileName);

            var all = playback.GetObservable<PerformanceSample>();

            var processor = PivotToInstanceSnapshots(playback, "Processor");
            var disk = PivotToInstanceSnapshots(playback,"PhysicalDisk");

            playback.Run();

            Assert.AreEqual(3000, processor.Count()); // there are 5 instances: _Total, 0, 1, 2, 3
            Assert.AreEqual(600, disk.Count());
        }

        [TestMethod]
        public void BlgFirst()
        {
            var observable = PerfCounterObservable.FromFile(BlgFileName);

            var result = observable.FirstAsync().Wait();

            Assert.IsNotNull(result);

            Assert.AreEqual(@"Avg. Disk Bytes/Read", result.CounterName, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("PhysicalDisk", result.CounterSet, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("0 C:", result.Instance, false, CultureInfo.InvariantCulture);
            Assert.AreEqual(DateTimeKind.Local, result.Timestamp.Kind);
            //Assert.AreEqual(new DateTimeOffset(634969254188440000, TimeSpan.Zero), result.Timestamp.ToUniversalTime());
        }

        [TestMethod]
        public void PerformanceCounterProbeFirst()
        {
            PerformanceSample[] result;
            var startTime = DateTimeOffset.UtcNow;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => PerfCounterObservable.FromRealTime(TimeSpan.FromSeconds(1), @"\Processor(_Total)\% User Time").Take(1),
                    typeof(PerfCounterPartitionTypeMap),
                    typeof(PerfCounterTypeMap));

                var query = playback.GetObservable<PerformanceSample>();

                var enumerable = playback.BufferOutput(query);

                playback.Run();

                result = enumerable.ToArray();
            }
            var endTime = DateTimeOffset.UtcNow;

            Assert.AreEqual(1, result.Length);

            Assert.AreEqual("% User Time", result[0].CounterName, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("Processor", result[0].CounterSet, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("_Total", result[0].Instance, false, CultureInfo.InvariantCulture);
            Assert.AreEqual(DateTimeKind.Local, result[0].Timestamp.Kind);
            var dto = new DateTimeOffset(result[0].Timestamp);
            Assert.IsTrue(dto >= startTime);
            Assert.IsTrue(dto <= endTime);
        }

        [TestMethod]
        public void PerformanceCounterProbeBadCounterName()
        {
            Exception error = null;
            using (PerfCounterObservable.FromRealTime(TimeSpan.FromHours(1), "blah")
                    .Subscribe(Observer.Create<PerformanceSample>(_ => { }, e => error = e)))
            {
                Assert.IsNotNull(error);
                Assert.IsInstanceOfType(error, typeof(Exception));
                Assert.AreEqual("PDH_CSTATUS_BAD_COUNTERNAME", error.Message);
            }
        }

        IEnumerable<InstanceCounterSnapshot> PivotToInstanceSnapshots(Playback playback, string counterSet)
        {
            var all = playback.GetObservable<PerformanceSample>();

            var instanceSnapshots = from a in all
                          where a.CounterSet == counterSet
                          group a by new { a.Machine, a.Instance, a.Timestamp } into groups
                          from g in groups.ToArray()
                          select new InstanceCounterSnapshot
                          {
                              Machhine = groups.Key.Machine,
                              Instance = groups.Key.Instance,
                              Timestamp = groups.Key.Timestamp,
                              Counters = g
                          };

            return playback.BufferOutput(instanceSnapshots);
        }

        class InstanceCounterSnapshot
        {
            public string Machhine { get; set; }
            public string Instance { get; set; }
            public DateTime Timestamp { get; set; }
            public PerformanceSample[] Counters { get; set; }
        }
    }
}
