using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reactive;
using System.Reflection;
using Tx.Windows;

namespace Tests.Tx
{
    [TestClass]
    public class TypeStatisticsTest
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
        public void EtwTypeStatistics()
        {
            var stat = new TypeOccurenceStatistics(Assembly.GetExecutingAssembly().GetTypes());
            stat.AddEtlFiles(EtlFileName);
            stat.Run();

            Assert.AreEqual(12, stat.Statistics.Count);
        }

        [TestMethod]
        public void EvtxTypeStatistics()
        {
            var stat = new TypeOccurenceStatistics(Assembly.GetExecutingAssembly().GetTypes());
            stat.AddLogFiles(EvtxFileName);
            stat.Run();

            Assert.AreEqual(12, stat.Statistics.Count);
        }
    }
}
