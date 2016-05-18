namespace LinqPadDriver.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;
    using Tx.Bond.LinqPad;

    [TestClass]
    public class TypeCacheTests
    {
        private string testDataFile = @"TestData\Triangles.etl";

        [TestMethod]
        public void Construct()
        {
            Assert.IsNotNull(new TypeCache());
        }

        [TestMethod]
        public void ResolveCacheDirectory()
        {
            var result = TypeCache.ResolveCacheDirectory("dir");

            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void Resolve()
        {
            var result = TypeCache.Resolve("dir");

            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetTypes()
        {
            var result = TypeCache.GetTypes("new");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void TypeCacheInitHappyPathTest()
        {
            var typeCache = new TypeCache();
            typeCache.Init("test", new[] { testDataFile });

            var dir = typeCache.CacheDirectory;

            Assert.AreEqual(3, Directory.GetFiles(dir, "*.bond").Length);
            Assert.AreEqual(3, typeCache.Manifests.Length);

            Directory.Delete(dir, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeCacheInitNoFilesTest()
        {
            var typeCache = new TypeCache();
            typeCache.Init("dir", new string[] { });
            typeCache.Init("dir", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeCacheInitNullFilesTest()
        {
            var typeCache = new TypeCache();
            typeCache.Init("dir", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeCacheInitTargetDirNullTest()
        {
            var typeCache = new TypeCache();
            typeCache.Init(null, new string[] { testDataFile });
        }

        [TestMethod]
        public void TypeCacheParseClassNamesHappyPathTest()
        {
            var testManifest = @"namespace DataModel

struct EquilateralTriangle
{
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};";
            var typeCache = new TypeCache();
            var tuple = typeCache.ParseClassNames(testManifest);

            Assert.AreEqual("DataModel", tuple.Item1);
            Assert.AreEqual("EquilateralTriangle", tuple.Item2[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeCacheParseClassNamesNullTest()
        {
            var typeCache = new TypeCache();
            typeCache.ParseClassNames(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeCacheParseClassNamesEmptyManifestTest()
        {
            var typeCache = new TypeCache();
            typeCache.ParseClassNames(string.Empty);
        }

        [TestMethod]
        // https://tx.codeplex.com/workitem/5
        public void TypeCacheParseStructNameHasCurlyBracesHappyPathTest()
        {
            var testManifest = @"namespace DataModel

struct EquilateralTriangle{
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};
struct SomeOtherTriangle{
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};";
            var typeCache = new TypeCache();
            var tuple = typeCache.ParseClassNames(testManifest);

            Assert.AreEqual("DataModel", tuple.Item1);
            Assert.AreEqual("EquilateralTriangle", tuple.Item2[0]);
            Assert.AreEqual("SomeOtherTriangle", tuple.Item2[1]);
        }

        [TestMethod]
        // https://tx.codeplex.com/workitem/5
        public void TypeCacheParseStructNameIgnoresInvalidTypesTest()
        {
            var testManifest = @"namespace DataModel

struct EquilateralTriangle{
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};
struct InvalidTriangle}
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};
struct ValidTriangle{
	1: optional		int64		Side;
	2: optional     string		Source;
	
	[OccurenceTime("")]
	3: optional     int64		TimeStampUtc;
};";
            var typeCache = new TypeCache();
            var tuple = typeCache.ParseClassNames(testManifest);

            Assert.AreEqual("DataModel", tuple.Item1);
            Assert.AreEqual("EquilateralTriangle", tuple.Item2[0]);
            Assert.AreEqual("ValidTriangle", tuple.Item2[1]);
        }

        [TestMethod]
        // https://tx.codeplex.com/workitem/6
        public void TypeCacheIgnoresTypesContainingImportTest()
        {
            var typeCache = new TypeCache();
            typeCache.Init("test5", new[] { @"TestData\TrianglesWithImport.etl" });

            Assert.AreEqual(4, typeCache.Types.Length);
            Assert.AreEqual(3, typeCache.Manifests.Length);
        }
        
    }
}
