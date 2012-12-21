using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace Tests.Tx
{
    [TestClass]
    public class HttpQueriesTest
    {
        string EtlFileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.etl");
            }
        }

        [TestMethod]
        public void HTTP_Parse()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var parsed = from p in pb.GetObservable<Parse>()
                         select new
                         {
                             p.Header.ActivityId,
                             p.Url
                         };

            int count = 0;
            parsed.Count().Subscribe(c => count = c);
            pb.Run();

            Assert.AreEqual(291, count);
        }

        [TestMethod]
        public void HTTP_FastSend()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var parsed = from s in pb.GetObservable<FastSend>()
                         select new
                        {
                            s.Header.ActivityId,
                            s.HttpStatus
                        };

            int count = 0;
            parsed.Count().Subscribe(c => count = c);
            pb.Run();

            Assert.AreEqual(289, count);
        }

        [TestMethod]
        public void HTTP_WholeRequest()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName); 
            var begin = pb.GetObservable<Parse>();
            var end = pb.GetObservable<FastSend>();

            var requests = from b in begin
                           from e in end.Where(e => e.Header.ActivityId == b.Header.ActivityId).Take(1)
                           select new
                           {
                               b.Header.ActivityId,
                               b.Url,
                               e.HttpStatus,
                               Duration = e.Header.Timestamp - b.Header.Timestamp
                           };

            int count = 0;
            requests.Subscribe(r=>count++);
            pb.Run();

            Assert.AreEqual(289, count);
        }

        [TestMethod]
        public void HTTP_AggregateDuration()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var begin = pb.GetObservable<Parse>();
            var end = pb.GetObservable<FastSend>();

            var requests = from b in begin
                           from e in end.Where(e => e.Header.ActivityId == b.Header.ActivityId).Take(1)
                           select new
                           {
                               b.Header.ActivityId,
                               b.Url,
                               e.HttpStatus,
                               Duration = e.Header.Timestamp - b.Header.Timestamp
                           };

            var statistics = from r in requests
                             group r by new
                             {
                                 Milliseconds = Math.Ceiling(r.Duration.TotalMilliseconds * 10) / 10,
                                 Url = r.Url
                             } into groups
                             from c in groups.Count()
                             select new
                             {
                                 groups.Key.Url,
                                 groups.Key.Milliseconds,
                                 Count = c
                             };


            var list = new List<object>();
            statistics.Subscribe(s => list.Add(s));

            pb.Run();

            Assert.AreEqual(7, list.Count());
        }

        [TestMethod]
        public void HTTP_SlowRequests()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var begin = pb.GetObservable<Parse>();
            var end = pb.GetObservable<FastSend>();

            var requests = from b in begin
                           from e in end.Where(e => e.Header.ActivityId == b.Header.ActivityId).Take(1)
                           select new
                           {
                               b.Header.ActivityId,
                               b.Url,
                               e.HttpStatus,
                               Duration = e.Header.Timestamp - b.Header.Timestamp
                           };

            var slow = from r in requests
                       where r.Duration.TotalMilliseconds > 0.5
                       select r;

            var list = new List<object>();
            slow.Subscribe(s => list.Add(s));

            pb.Run();

            Assert.AreEqual(2, list.Count());
        }

        [TestMethod]
        public void HTTP_SinglePass()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);

            var begin = pb.GetObservable<Parse>();
            var end = pb.GetObservable<FastSend>();

            var requests = from b in begin
                           from e in end.Where(e => e.Header.ActivityId == b.Header.ActivityId).Take(1)
                           select new
                           {
                               b.Header.ActivityId,
                               b.Url,
                               e.HttpStatus,
                               Duration = e.Header.Timestamp - b.Header.Timestamp
                           };

            var statistics = from r in requests
                             group r by new
                             {
                                 Milliseconds = Math.Ceiling(r.Duration.TotalMilliseconds * 10) / 10,
                                 Url = r.Url
                             } into groups
                             from c in groups.Count()
                             select new
                             {
                                 groups.Key.Url,
                                 groups.Key.Milliseconds,
                                 Count = c
                             };

            var slow = from r in requests
                       where r.Duration.TotalMilliseconds > 0.5
                       select r;

            var slowList = new List<object>();
            slow.Subscribe(s => slowList.Add(s));

            var statisticsList = new List<object>();
            statistics.Subscribe(s => statisticsList.Add(s));

            pb.Run();

            Assert.AreEqual(7, statisticsList.Count());
            Assert.AreEqual(2, slowList.Count());
        }

        [TestMethod]
        public void HTTP_Parse_Format()
        {
            string msg = ""; 
            
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);

            var parsed = pb.GetObservable<Parse>().Take(1);
            parsed.Subscribe(p => msg = p.ToString());
            pb.Run();

            Assert.AreEqual(msg, "Parsed request (request pointer 18446738026454074672, method 4) with URI http://georgis2:80/windir.txt");
        }
    }
}
