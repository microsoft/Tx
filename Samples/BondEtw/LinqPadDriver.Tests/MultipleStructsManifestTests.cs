using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Bond.LinqPad;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace LinqPadDriver.Tests
{
    [TestClass]
    public class MultipleStructsManifestTests
    {
        private string testDataFile = @"TestData\Employees.etl";

        [TestMethod]
        public void ReadTypesFromSingleManifestTest()
        {
            TypeCache typecache = new TypeCache();
            typecache.Initialize("ReadTypesFromSingleManifestTest", new[] { testDataFile });

            Debug.WriteLine(typecache.CacheDirectory);

            Assert.AreEqual(4, typecache.Cache.Count);

            Debug.WriteLine(typecache.Cache[0].Manifest.ManifestId);
            Debug.WriteLine(typecache.Cache[1].Manifest.ManifestId);
            Debug.WriteLine(typecache.Cache[2].Manifest.ManifestId);
            Debug.WriteLine(typecache.Cache[3].Manifest.ManifestId);

            Debug.WriteLine(typecache.Cache[0].Type.Name);
            Debug.WriteLine(typecache.Cache[1].Type.Name);
            Debug.WriteLine(typecache.Cache[2].Type.Name);
            Debug.WriteLine(typecache.Cache[3].Type.Name);

            Assert.IsTrue(typecache.Cache.All(a => a != null && a.Manifest != null && a.Type != null));
        }

        [TestMethod]
        public void MultipleTypesStatsCalculationTest()
        {
            TypeCache typecache = new TypeCache();
            typecache.Initialize("MultipleTypesStatsCalculationTest", new[] { testDataFile });

            Assert.AreEqual(4, typecache.Cache.Count);
            Assert.IsTrue(typecache.Cache.All(a => a != null && a.Manifest != null && a.Type != null));
            
            EventStatisticCache cache = new EventStatisticCache();
            var result = cache.GetTypeStatistics(typecache, new[] { testDataFile });

            Assert.AreEqual(3, result.Count);

            var values = result.Values.ToArray();
            Assert.AreEqual(10, values[0].EventCount);
            Assert.AreEqual(10, values[1].EventCount);
            Assert.AreEqual(10, values[2].EventCount);

            File.Delete(Path.Combine("TestData", Path.GetFileNameWithoutExtension(testDataFile) + ".csv"));
        }
    }
}
