using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Reflection;
using Tx.Windows;

namespace Tests.Tx
{
    [TestClass]
    public class EvtxTest
    {
        string FileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"HTTP_Server.evtx");
            }
        }

        [TestMethod]
        public void EvtxReader()
        {
            var parser = EvtxEnumerable.FromFiles(FileName);
            int count = parser.Count();

            Assert.AreEqual(2041, count); // in ETW there is one more event with system information
        }
    }
}
