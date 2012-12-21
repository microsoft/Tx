using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace Tests.Tx
{
    [TestClass]
    public class EvtxDeserializerTestcs
    {
        string FileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.etl");
            }
        }

        //[TestMethod]
        //public void EvtxManifestDeserializerOne()
        //{
        //    var subject = new Subject<Timestamped<object>>();
        //    var deserializer = new PartitionKeyDeserializer<EventRecord, ManifestEventPartitionKey>(
        //        new EvtxManifestTypeMap(),
        //        subject);

        //    deserializer.AddKnownType(typeof(Deliver));

        //    ManualResetEvent completed = new ManualResetEvent(false);
        //    int count = 0;
        //    subject.Subscribe(
        //        p =>
        //        {
        //            count++;
        //        },
        //        () =>
        //        {
        //            completed.Set();
        //        });

        //    var input = EvtxEnumerable.FromFiles(FileName).ToObservable();
        //    input.Subscribe(deserializer);
        //    completed.WaitOne();

        //    Assert.AreEqual(291, count);
        //}

        //[TestMethod]
        //public void EvtxanifestDeserializerMany()
        //{
        //    var subject = new Subject<Timestamped<object>>();
        //    var deserializer = new PartitionKeyDeserializer<EventRecord, ManifestEventPartitionKey>(
        //        new EvtxManifestTypeMap(),
        //        subject); 

        //    deserializer.AddKnownType(typeof(Deliver));
        //    deserializer.AddKnownType(typeof(FastResp));

        //    ManualResetEvent completed = new ManualResetEvent(false);
        //    int count = 0;
        //    subject.Subscribe(
        //        p =>
        //        {
        //            count++;
        //        },
        //        () =>
        //        {
        //            completed.Set();
        //        });

        //    var input = EvtxEnumerable.FromFiles(FileName).ToObservable();
        //    input.Subscribe(deserializer);
        //    completed.WaitOne();

        //    Assert.AreEqual(580, count);
        //}
    }
}
