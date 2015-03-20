using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reactive;
using System.Reflection;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace Tests.Tx
{
    using System.Collections.Generic;
    using System.Reactive.Linq;

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

        [TestMethod]
        public void MergeTwoStreams()
        {
            var result = new List<string>();

            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var playback = new Playback())
            {
                playback.AddInput(new[] 
		        {
			        new Timestamped<object>(1, start),
			        new Timestamped<object>(2, start.AddSeconds(2)),
			        new Timestamped<object>("3", start.AddSeconds(3)),
		        });

                using (playback
                    .GetObservable<int>()
                    .Select(i => i.ToString())
                    .Merge(playback.GetObservable<string>(), playback.Scheduler)
                    .Subscribe(Observer.Create<string>(result.Add)))
                {
                    playback.Run();
                }
            }

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
        }
    }
}
