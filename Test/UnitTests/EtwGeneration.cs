using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Tests.Tx
{
    [TestClass]
    public class EtwGeneration
    {
        string ToolFileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"EtwEventTypeGen.exe");
            }
        }
        
        [TestMethod]
        public void GenerateHttp()
        {
            
            var tool = Process.Start(ToolFileName, "/m:HTTP_Server.man /o:http");
            tool.WaitForExit();

            Assert.AreEqual(0, tool.ExitCode);
        }

        [TestMethod]
        public void GenerateAsp()
        {

            var tool = Process.Start(ToolFileName, "/m:asp.man /o:http");
            tool.WaitForExit();

            Assert.AreEqual(0, tool.ExitCode);
        }
    }
}
