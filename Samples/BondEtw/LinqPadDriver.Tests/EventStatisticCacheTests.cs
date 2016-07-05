using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Bond.LinqPad;
using System.Linq;
using System.IO;
using System.Threading;

namespace LinqPadDriver.Tests
{
    [TestClass]
    public class EventStatisticCacheTests
    {
        private string testDataFile = @"TestData\Triangles.etl";

        [TestInitializeAttribute]
        public void InitializeTest()
        {
            var testDir = @"TestData";
            var staleCsv = Directory.EnumerateFiles(testDir, "*.csv").ToList();

            staleCsv.ForEach(a => File.Delete(a));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EventStatisticCacheGetTypeStatisticsNullTypesTest()
        {
            EventStatisticCache cache = new EventStatisticCache();
            cache.GetTypeStatistics(null, testDataFile);
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EventStatisticCacheGetTypeStatisticsFilesInvalidTest()
        {
            TypeCache typecache = new TypeCache();

            EventStatisticCache cache = new EventStatisticCache();
            cache.GetTypeStatistics(typecache, new[] { string.Empty, testDataFile });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EventStatisticCacheGetTypeStatisticsIncorrectFilePathsTest()
        {
            TypeCache typecache = new TypeCache();

            EventStatisticCache cache = new EventStatisticCache();
            cache.GetTypeStatistics(typecache, new[] { @"C:\hello\world.etl", testDataFile });

        }

        [TestMethod]
        public void EventStatisticCacheGetTypeStatisticsHappyPathTest()
        {
            TypeCache typecache = new TypeCache();
            typecache.Initialize("test1", new[] { testDataFile });

            EventStatisticCache cache = new EventStatisticCache();
            var result = cache.GetTypeStatistics(typecache, new[] { testDataFile });
            
            Assert.AreEqual(3, result.Keys.Count);
            Assert.IsTrue(result.All(a => a.Value.EventCount == 10));

            var typeNames = new[] { "IsoscelesTriangle", "EquilateralTriangle", "RightAngledTriangle" };
            Assert.IsTrue(result.All(a => typeNames.Contains(a.Key.Name)));
        }

        [TestMethod]
        public void EventStatisticCacheCreatesCsvTest()
        {
            TypeCache typecache = new TypeCache();
            typecache.Initialize("test2", new[] { testDataFile });

            EventStatisticCache cache = new EventStatisticCache();
            var result = cache.GetTypeStatistics(typecache, new[] { testDataFile });

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "TestData", Path.GetFileNameWithoutExtension(testDataFile) + ".csv")));

            File.Delete(Path.Combine(Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(testDataFile) + ".csv"));

        }

        [TestMethod]
        public void EventStatisticLoadCsvValuesTest()
        {
            TypeCache typecache = new TypeCache();
            typecache.Initialize("test3", new[] { testDataFile });

            EventStatisticCache cache = new EventStatisticCache();
            var result = cache.GetTypeStatistics(typecache, new[] { testDataFile });

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "TestData", Path.GetFileNameWithoutExtension(testDataFile) + ".csv")));

            File.Delete(Path.Combine(Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(testDataFile) + ".csv"));

            foreach (var item in result)
            {
                switch (item.Key.Name)
                {
                    case "IsoscelesTriangle":
                        Assert.AreEqual(27D, item.Value.AverageByteSize);
                        Assert.AreEqual(270, item.Value.ByteSize);
                        Assert.AreEqual(10, item.Value.EventCount);
                        Assert.AreEqual(0.0370, item.Value.EventsPerSecond);
                        break;

                    case "EquilateralTriangle":
                        Assert.AreEqual(25D, item.Value.AverageByteSize);
                        Assert.AreEqual(250, item.Value.ByteSize);
                        Assert.AreEqual(10, item.Value.EventCount);
                        Assert.AreEqual(0.0370, item.Value.EventsPerSecond);
                        break;

                    case "RightAngledTriangle":
                        Assert.AreEqual(29D, item.Value.AverageByteSize);
                        Assert.AreEqual(290, item.Value.ByteSize);
                        Assert.AreEqual(10, item.Value.EventCount);
                        Assert.AreEqual(0.037, item.Value.EventsPerSecond);
                        break;

                    default:
                        Assert.Fail("Unexpected test result. Test fails.");
                        break;
                }
                
            }
        }
    }
}
