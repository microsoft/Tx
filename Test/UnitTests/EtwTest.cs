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
        public void EtwObservableFirst()
        {
            var observable = EtwObservable.FromFiles(FileName);

            string failureMessage = null;

            observable
                .Take(1)
                .Do(
                nativeEvent =>
                {
                    try
                    {
                        Assert.AreEqual(new Guid("00000100-0000-0003-193d-42fb30bbcb01"), nativeEvent.ActivityId);
                        Assert.AreEqual(16, nativeEvent.Channel);
                        Assert.AreEqual(0, nativeEvent.EventProperty);
                        Assert.IsNotNull(nativeEvent.ExtendedData);
                        Assert.AreEqual(0, nativeEvent.ExtendedDataCount);
                        Assert.AreEqual(576, nativeEvent.Flags);
                        Assert.AreEqual(0, nativeEvent.HeaderType);
                        Assert.AreEqual(21, nativeEvent.Id);
                        Assert.AreEqual(9223372036854775824L, nativeEvent.Keyword);
                        Assert.AreEqual(4, nativeEvent.Level);
                        Assert.AreEqual(28, nativeEvent.Opcode);
                        Assert.AreEqual((uint)0, nativeEvent.ProcessId);
                        Assert.AreEqual((uint)677443, nativeEvent.ProcessorTime);
                        Assert.AreEqual((ushort)152, nativeEvent.Size);
                        Assert.AreEqual((ushort)4, nativeEvent.Task);
                        Assert.AreEqual(new Guid("dd5ef90a-6398-47a4-ad34-4dcecdef795f"), nativeEvent.ProviderId);
                        Assert.AreEqual((uint)0, nativeEvent.ThreadId);
                        Assert.AreEqual(DateTimeOffset.FromFileTime(129402940472257591L), nativeEvent.TimeStamp);
                        Assert.AreEqual(129402940472257591L, nativeEvent.TimeStampRaw);
                        Assert.IsNotNull(nativeEvent.UserContext);
                        Assert.IsNotNull(nativeEvent.UserData);
                        Assert.AreEqual((ushort)72, nativeEvent.UserDataLength);
                        Assert.AreEqual(0, nativeEvent.Version);
                    }
                    catch (AssertFailedException assertError)
                    {
                        failureMessage = assertError.Message;
                    }
                })
                .Wait();

            if (failureMessage != null)
            {
                Assert.Fail(failureMessage);
            }
        }

        [TestMethod]
        public void EtwObservableTest()
        {
            var observable = EtwObservable.FromFiles(FileName);

            int count = observable.Count().Wait();

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
