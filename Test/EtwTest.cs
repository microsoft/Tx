using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using Tx.Windows;

namespace Tests.Tx
{
    [TestClass]
    public class EtwTest
    {
        string FileName 
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.etl");
            }
        }

        [TestMethod]
        public void EtwObservableTest()
        {
            var observable = EtwObservable.FromFiles(FileName);

            int count = 0;

            observable.ForEach(
                x =>
                {
                    count++;
                });

            Assert.AreEqual(2041, count);
        }

        [TestMethod]
        public void EtwFileSourceTest()
        {
            var observable = EtwObservable.FromFiles(FileName);
            var source = new TimeSource<EtwNativeEvent>(observable, e => e.TimeStamp);

             var parsed = from p in source
                        where p.Id == 2
                        select p.TimeStamp;

            var buf = parsed.Take(13).Buffer(TimeSpan.FromSeconds(1), source.Scheduler);

            var list = new List<IList<DateTimeOffset>>();
            ManualResetEvent completed = new ManualResetEvent(false);

            buf.Subscribe(
                t => list.Add(t), 
                ()=>completed.Set());

            source.Connect();
            completed.WaitOne();

            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(7, list.First().Count);
            Assert.AreEqual(6, list.Skip(1).First().Count);
        }

        [TestMethod]
        public void EtwParser()
        {
            var parser = EtwObservable.FromFiles(FileName);

            int count=0;
            ManualResetEvent completed = new ManualResetEvent(false);

            parser.Count().Subscribe(
                x => count = x,
                ()=> completed.Set());

            completed.WaitOne();

            Assert.AreEqual(2041, count);
        }
    }
}
