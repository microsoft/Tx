using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Tx.Windows;

namespace Tests.Tx
{
    [TestClass]
    public class EventNormalizerTest
    {
        string EtlFileName 
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.etl");
            }
        }

        string EvtxFileName 
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.evtx");
            }
        }
        
        [TestMethod]
        public void EtlEventTypeStatistics()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var statistics = from e in pb.GetObservable<SystemEvent>()
                             group e by new { e.Header.ProviderId, e.Header.EventId, e.Header.Opcode, e.Header.Version }
                                 into g
                                 from c in g.Count()
                                 select new
                                 {
                                     g.Key.ProviderId,
                                     g.Key.EventId,
                                     g.Key.Version,
                                     Count = c,
                                 };

            var list = new List<object>();
            statistics.Subscribe(s => list.Add(s));

            pb.Run();

            Assert.AreEqual(12, list.Count());
        }

        [TestMethod]
        public void EvtxEventTypeStatistics()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var statistics = from e in pb.GetObservable<SystemEvent>()
                             group e by new { e.Header.ProviderId, e.Header.EventId, e.Header.Opcode, e.Header.Version }
                                 into g
                                 from c in g.Count()
                                 select new
                                 {
                                     g.Key.ProviderId,
                                     g.Key.EventId,
                                     g.Key.Version,
                                     Count = c,
                                 };

            var list = new List<object>();
            statistics.Subscribe(s => list.Add(s));

            pb.Run();

            Assert.AreEqual(12, list.Count());
        }

        [TestMethod]
        public void BothEtlAndEvtx()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var statistics = from e in pb.GetObservable<SystemEvent>()
                             group e by new { e.Header.ProviderId, e.Header.EventId, e.Header.Opcode, e.Header.Version }
                                 into g
                                 from c in g.Count()
                                 select new
                                 {
                                     g.Key.ProviderId,
                                     g.Key.EventId,
                                     g.Key.Version,
                                     Count = c,
                                 };

            var list = new List<object>();
            statistics.Subscribe(s => list.Add(s));

            pb.Run();

            Assert.AreEqual(12, list.Count());
        }
    }
}
