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
    using System.Reactive.Subjects;
    using System.Threading;

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
        public void OfType()
        {
            var count = EtwObservable.FromFiles(this.EtlFileName)
                .OfType<EtwNativeEvent, Parse>(
                    new EtwManifestTypeMap(),
                    new EtwClassicTypeMap(),
                    new EtwTypeMap())
                .Count()
                .Wait();

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

            Assert.AreEqual(2041+291, count);
            Assert.AreEqual(291, parseCount);
        }

        [TestMethod]
        public void MergeTwoStreams_1()
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

        [TestMethod]
        public void MergeEmptyWindowAndEmptyStream()
        {
            var result = new List<object>();
            var now = DateTimeOffset.UtcNow;

            using (var playback = new Playback())
            {
                playback.AddInput(new[] { new Timestamped<object>(4L, now), });
                using (playback.GetObservable<string>()
                        .Window(TimeSpan.FromSeconds(40), playback.Scheduler)
                        .Merge(Observable.Empty<object>(), playback.Scheduler)
                        .Subscribe(Observer.Create<object>(result.Add)))
                {
                    playback.Run();
                }
            }

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        //[Ignore]
        public void MergeTwoStreams_2()
        {
            var result = new List<string>();
            bool isCompleted = false;
            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var playback = new Playback())
            {
                playback.StartTime = start.UtcDateTime;

                playback.AddInput(new[] 
		        {
			        new Timestamped<object>("0", start),
                    //new Timestamped<object>("1", start.AddSeconds(1)),
                    //new Timestamped<object>("2", start.AddSeconds(2)),
                    //new Timestamped<object>("3", start.AddSeconds(3)),
		        });

                playback.AddInput(new[] 
		        {
//			        new Timestamped<object>(new {}, start),
			        new Timestamped<object>("1", start.AddSeconds(1)),
			        new Timestamped<object>("2", start.AddSeconds(2)),
			        new Timestamped<object>("3", start.AddSeconds(3)),
		        });


                using (playback.GetObservable<int>()
                    .Select(i => i.ToString())
                    .Merge(playback.GetObservable<string>(), playback.Scheduler)
                    .Subscribe(Observer.Create<string>(result.Add, () => isCompleted = true)))
                {
                    playback.Run();
                }
            }

            Assert.IsTrue(isCompleted);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [TestMethod]
        public void MergeTwoStreams_3()
        {
            var result = new List<string>();

            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var playback = new Playback())
            {
                playback.AddInput(new[] 
		        {
			        new Timestamped<object>(1, start),
			        new Timestamped<object>(2, start.AddSeconds(2)),
			        new Timestamped<object>(3, start.AddSeconds(3)),
		        });

                using (playback.GetObservable<int>()
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

        [TestMethod]
        public void MergeTwoStreams_4()
        {
            var result = new List<string>();
            var errors = new List<Exception>();

            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var waitHandle = new ManualResetEvent(false))
            using (var subject = new Subject<Timestamped<int>>())
            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => subject,
                    new ITypeMap<Timestamped<int>>[] { new TimestampedTypeMap<int>() });

                using (playback.GetObservable<int>()
                    .Select(i => i.ToString())
                    .Merge(playback.GetObservable<string>(), playback.Scheduler)
                    .Subscribe(
                        Observer.Create<string>(
                            result.Add, 
                            errors.Add,
                            () => waitHandle.Set())))
                {
                    playback.Start();

                    subject.OnNext(new Timestamped<int>(1, start));

                    Assert.AreEqual(1, result.Count);

                    subject.OnCompleted();

                    var notTimeouted = waitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    Assert.IsTrue(notTimeouted);
                }
            }

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1", result[0]);
        }

        [TestMethod]
        [Ignore]
        public void MergeTwoStreams_5()
        {
            var result = new List<string>();
            var errors = new List<Exception>();

            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var waitHandle = new ManualResetEvent(false))
            using (var subject = new Subject<Timestamped<int>>())
            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => subject,
                    new ITypeMap<Timestamped<int>>[] { new TimestampedTypeMap<int>() });

                using (playback.GetObservable<string>()
                    .Merge(playback.GetObservable<int>().Select(i => i.ToString()), playback.Scheduler)
                    .Subscribe(
                        Observer.Create<string>(
                            result.Add,
                            errors.Add,
                            () => waitHandle.Set())))
                {
                    playback.Start();

                    subject.OnNext(new Timestamped<int>(1, start));

                    Assert.AreEqual(1, result.Count);

                    subject.OnCompleted();

                    var notTimeouted = waitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    Assert.IsTrue(notTimeouted);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void MergeTwoStreams_6()
        {
            var result = new List<string>();
            var errors = new List<Exception>();

            using (var waitHandle = new ManualResetEvent(false))
            using (var intSubject = new Subject<Timestamped<int>>())
            using (var stringSubject = new Subject<Timestamped<string>>())
            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => intSubject,
                    new ITypeMap<Timestamped<int>>[] { new TimestampedTypeMap<int>() });

                ((IPlaybackConfiguration)playback).AddInput(
                    () => stringSubject,
                    new ITypeMap<Timestamped<string>>[] { new TimestampedTypeMap<string>() });

                using (playback.GetObservable<int>().Select(i => i.ToString())
                    .Merge(playback.GetObservable<string>(), playback.Scheduler)
                    .Subscribe(
                        Observer.Create<string>(
                            result.Add,
                            errors.Add,
                            () => waitHandle.Set())))
                {
                    playback.Start();

                    intSubject.OnNext(new Timestamped<int>(1, DateTimeOffset.UtcNow));

                    Assert.AreEqual(1, result.Count);

                    intSubject.OnCompleted();
                    stringSubject.OnCompleted();

                    var notTimeouted = waitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    Assert.IsTrue(notTimeouted);
                }
            }

            Assert.AreEqual(0, errors.Count);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1", result[0]);
        }

        [TestMethod]
        public void MergeTwoStreams_7()
        {
            var result = new List<string>();
            var errors = new List<Exception>();

            var start = new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);

            using (var waitHandle = new ManualResetEvent(false))
            using (var intSubject = new Subject<Timestamped<int>>())
            using (var stringSubject = new Subject<Timestamped<string>>())
            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => stringSubject,
                    new ITypeMap<Timestamped<string>>[] { new TimestampedTypeMap<string>() });

                ((IPlaybackConfiguration)playback).AddInput(
                    () => intSubject,
                    new ITypeMap<Timestamped<int>>[] { new TimestampedTypeMap<int>() });

                using (playback.GetObservable<string>()
                    .Merge(playback.GetObservable<int>().Select(i => i.ToString()), playback.Scheduler)
                    .Subscribe(
                        Observer.Create<string>(
                            result.Add,
                            errors.Add,
                            () => waitHandle.Set())))
                {
                    playback.Start();

                    stringSubject.OnNext(new Timestamped<string>("1", start));
                    intSubject.OnNext(new Timestamped<int>(2, start.AddTicks(1)));
                    stringSubject.OnNext(new Timestamped<string>("3", start.AddTicks(2)));
                    intSubject.OnNext(new Timestamped<int>(4, start.AddTicks(3)));

                    stringSubject.OnCompleted();
                    intSubject.OnCompleted();

                    var notTimeouted = waitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    Assert.IsTrue(notTimeouted);
                }
            }

            Assert.AreEqual(0, errors.Count);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
            Assert.AreEqual("4", result[3]);
        }

        [TestMethod]
        public void ErrorHandlingTest()
        {
            var errors = new List<Exception>();
            bool isCompleted = false;

            using (var playback = new Playback())
            {
                ((IPlaybackConfiguration)playback).AddInput(
                    () => new[] { new CompositeDeserializerTests.Envelope { Data = "data" } }.ToObservable(),
                    new EnvelopeTestTypeMap2(),
                    new SystemClockTypeMap<CompositeDeserializerTests.Envelope>());

                using (playback
                    .GetObservable<CompositeDeserializerTests.Envelope>()
                    .Subscribe(item => { }, error => errors.Add(error), () => isCompleted = true))
                {
                    playback.Run();
                }
            }

            Assert.IsFalse(isCompleted);
            Assert.AreEqual(1, errors.Count);
        }

        internal sealed class EnvelopeTestTypeMap2 : ITypeMap<CompositeDeserializerTests.Envelope>
        {
            public Func<CompositeDeserializerTests.Envelope, DateTimeOffset> TimeFunction
            {
                get
                {
                    return e => new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero);
                }
            }

            public Func<CompositeDeserializerTests.Envelope, object> GetTransform(Type outputType)
            {
                return envelope => envelope.Data;
            }
        }

    }
}
