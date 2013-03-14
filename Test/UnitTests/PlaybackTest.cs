using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reactive;
using System.Reflection;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace Tests.Tx
{
    [TestClass]
    public class PlaybackTest
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
        public void PlayOne()
        {
            var p = new Playback();
            p.AddEtlFiles(EtlFileName);

            int count = 0;
            p.GetObservable<Parse>().Subscribe(e => { count++; });
            p.Run();

            Assert.AreEqual(291, count);
        }

        [TestMethod]
        public void PlayTwo()
        {
            var p = new Playback();
            p.AddEtlFiles(EtlFileName);

            int parseCount = 0;
            int fastSendCount = 0;
            p.GetObservable<Deliver>().Subscribe(e => { parseCount++; });
            p.GetObservable<FastResp>().Subscribe(e => { fastSendCount++; });
            p.Run();

            Assert.AreEqual(291, parseCount);
            Assert.AreEqual(289, fastSendCount);
        }

        [TestMethod]
        public void PlayTwoBothEtlAndEvtx()
        {
            var p = new Playback();
            p.AddEtlFiles(EtlFileName);
            p.AddLogFiles(EvtxFileName);

            int parseCount = 0;
            int fastSendCount = 0;
            p.GetObservable<Deliver>().Subscribe(e => { parseCount++; });
            p.GetObservable<FastResp>().Subscribe(e => { fastSendCount++; });
            p.Run();

            Assert.AreEqual(582, parseCount);     
            Assert.AreEqual(578, fastSendCount);  
        }

        [TestMethod]
        public void PlayRoot()
        {
            var p = new Playback();
            p.AddEtlFiles(EtlFileName);

            int count = 0;
            p.GetObservable<SystemEvent>().Subscribe(e => { count++; });
            p.Run();

            Assert.AreEqual(2041, count);     
        }

        [TestMethod]
        public void PlayRootAndKnownType()
        {
            var p = new Playback();
            p.AddEtlFiles(EtlFileName);

            int count = 0;
            p.GetObservable<SystemEvent>().Subscribe(e => { count++; });
            int parseCount = 0;
            p.GetObservable<Deliver>().Subscribe(e => { parseCount++; });
            p.Run();

            Assert.AreEqual(2041, count);
            Assert.AreEqual(291, parseCount);
        }
    }
}
