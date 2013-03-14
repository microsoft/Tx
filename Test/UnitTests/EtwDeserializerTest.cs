using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace Tests.Tx
{
    [TestClass]
    public class EtwDeserializerTest
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
        //public void EtwManifestDeserializerOne()
        //{
        //    var subject = new Subject<Timestamped<object>>();
        //    var deserializer = new PartitionKeyDeserializer<EtwNativeEvent, ManifestEventPartitionKey>( 
        //        new EtwManifestTypeMap(), 
        //        subject);
        //    deserializer.AddKnownType(typeof(Parse));

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

        //    var input = EtwObservable.FromFiles(FileName);
        //    input.Subscribe(deserializer);
        //    completed.WaitOne();

        //    Assert.AreEqual(291, count);
        //}

        //[TestMethod]
        //public void EtwManifestDeserializerMany()
        //{
        //    var subject = new Subject<Timestamped<object>>();
        //    var deserializer = new PartitionKeyDeserializer<EtwNativeEvent, ManifestEventPartitionKey>(
        //        new EtwManifestTypeMap(),
        //        subject);        
        //    deserializer.AddKnownType(typeof(Parse));
        //    deserializer.AddKnownType(typeof(FastSend));
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

        //    var input = EtwObservable.FromFiles(FileName);
        //    input.Subscribe(deserializer);
        //    completed.WaitOne();

        //    Assert.AreEqual(871, count);
        //}
    }
}
