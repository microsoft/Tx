using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Tx.Windows;

namespace Tests.Tx
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

            int count = 0;

            observable.ForEach(
                x =>
                {
                    count++;
                });

            Assert.AreEqual(6000, count);
        }

        [TestMethod]
        public void BlgStatistics()
        {
            var observable = PerfCounterObservable.FromFile(BlgFileName);

            int count = 0;

            observable.Where(p=>p.CounterSet == "PhysicalDisk").ForEach(
                x =>
                {
                    count++;
                });

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

            var all = grouped.ToArray().First();

            Assert.AreEqual(600, all.Length);
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
