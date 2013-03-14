using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Process;

namespace Tests.Tx
{
    [TestClass]
    public class ProcessTest
    {
        string EtlFileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"Process.etl");
            }
        }

        [TestMethod]
        public void ProcessStartTest()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var start = from p in pb.GetObservable<ProcessStart_V0>() 
                         select new
                         {
                             p.ProcessID,
                             p.ImageName
                         };

            int count = 0;
            start.Count().Subscribe(c => count = c);
            pb.Run();

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void ProcessStopTest()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var stop = from p in pb.GetObservable<ProcessStop_V1>() 
                       select new
                       {
                           p.ProcessID,
                           p.ImageName
                       };

            int count = 0;
            stop.Count().Subscribe(c => count = c);
            pb.Run();

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void JoinStartStopTest()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);
            var start = pb.GetObservable<ProcessStart_V0>(); // this is V0 event
            var end = pb.GetObservable<ProcessStop_V1>(); // this is V1 event

            var processes = from s in start
                            from e in end.Where(e=>e.ProcessID == s.ProcessID).Take(1)
                            select new
                            {
                                s.ProcessID,
                                s.ImageName,
                                Duration = e.OccurenceTime - s.OccurenceTime
                            };

            int count = 0;
            processes.Count().Subscribe(c => count = c);
            pb.Run();

            Assert.AreEqual(2, count); // looked at the raw data with TraceInsigt...
        }
    }
}
