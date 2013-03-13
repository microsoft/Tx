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

            Assert.AreEqual(all.Length, 600);
        }
    }
}
