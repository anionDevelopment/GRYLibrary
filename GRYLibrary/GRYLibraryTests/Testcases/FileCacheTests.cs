using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class FileCacheTests
    {

        [TestMethod]
        public void FileCacheTestGet()
        {
            // arrange
            using TempFile tempFile = new TempFile();
            File.WriteAllLines(tempFile.Path, ["k1;v1", "k2;v2"]);
            FileCache<TestSerializable, TestSerializable> fileCache = new FileCache<TestSerializable, TestSerializable>(tempFile.Path);
            fileCache.Load();
            TestSerializable k2 = new TestSerializable();
            k2.DeserializeFromString("k2");
            TestSerializable expected = new TestSerializable();
            expected.DeserializeFromString("v2");

            // act
            TestSerializable actual = fileCache.Get(k2);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FileCacheTestSet()
        {
            // arrange
            using TempFile tempFile = new TempFile();
            File.WriteAllLines(tempFile.Path, ["k1;v1"]);
            FileCache<TestSerializable, TestSerializable> fileCache = new FileCache<TestSerializable, TestSerializable>(tempFile.Path);
            fileCache.Load();
            TestSerializable k2 = new TestSerializable();
            k2.DeserializeFromString("k2");
            TestSerializable v2 = new TestSerializable();
            v2.DeserializeFromString("v2");

            ISet<string> expected = new HashSet<string>() { "k1;v1", "k2;v2" };

            // act
            fileCache.Set(k2, v2);

            // assert
            string[] actual = File.ReadAllLines(tempFile.Path);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod]
        public void FileCacheTestRemove()
        {
            // arrange
            using TempFile tempFile = new TempFile();
            File.WriteAllLines(tempFile.Path, ["k1;v1", "k2;v2"]);
            FileCache<TestSerializable, TestSerializable> fileCache = new FileCache<TestSerializable, TestSerializable>(tempFile.Path);
            fileCache.Load();
            TestSerializable k2 = new TestSerializable();
            k2.DeserializeFromString("k2");

            ISet<string> expected = new HashSet<string>() { "k1;v1" };

            // act
            fileCache.Remove(k2);

            // assert
            string[] actual = File.ReadAllLines(tempFile.Path);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        private record TestSerializable : ISimpleSerializable
        {
            public string Value { get; set; }
            public void DeserializeFromString(string content)
            {
                this.Value = content;
            }

            public string SerializeToString()
            {
                return this.Value;
            }
        }
    }
}
