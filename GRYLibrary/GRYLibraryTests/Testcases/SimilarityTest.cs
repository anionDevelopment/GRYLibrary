using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class SimilarityTest
    {
        #region Combined
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestCalculateCombinedSimilarity1()
        {
            this.TestRange(0.8.ToPercentValue(), Similarity.CalculateCombinedSimilarity("123a5678", "123b5678"), 0.99.ToPercentValue());
            this.TestRange(0.01.ToPercentValue(), Similarity.CalculateCombinedSimilarity("12345678", "a2cdefgh"), 0.2.ToPercentValue());
            Assert.AreEqual(0, Similarity.CalculateCombinedSimilarity("1234", "56789").Value);
            Assert.AreEqual(1, Similarity.CalculateCombinedSimilarity("1234", "1234").Value);
        }
        #endregion 
        #region Jaccard
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestCalculateJaccardSimilarityWithEqualStrings()
        {
            string testString = "test";
            Assert.AreEqual(1, Similarity.CalculateJaccardSimilarity(testString, testString).Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestJaccardIndexWithEqualStrings()
        {
            string testString = "test";
            Assert.AreEqual(0.5, Similarity.CalculateJaccardIndex(testString, testString));
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestJaccardSimilarityWithDifferentValues()
        {
            Assert.AreEqual(4 / (decimal)5, Similarity.CalculateJaccardSimilarity("test1", "test2").Value);
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity("test", string.Empty).Value);
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity(string.Empty, "test").Value);
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity("test", "a").Value);
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity("a", "test").Value);
            Assert.AreEqual((decimal)1 / 4, Similarity.CalculateJaccardSimilarity("test", "aeaa").Value);
            Assert.AreEqual((decimal)1 / 4, Similarity.CalculateJaccardSimilarity("aeaa", "test").Value);
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity("abcd", "efgh").Value);
            Assert.AreEqual((decimal)3 / 8, Similarity.CalculateJaccardSimilarity("xx345xxx", "yy345yyy").Value);
            Assert.AreEqual((decimal)5 / 8, Similarity.CalculateJaccardSimilarity("12xxx678", "12yyy678").Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestJaccardIndexSimilarityWithCompletelyDifferentStrings()
        {
            Assert.AreEqual(0, Similarity.CalculateJaccardSimilarity("abcd", "efgh").Value);
        }
        #endregion
        #region Levenshtein
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestLevenshteinSimilarityWithEqualStrings()
        {
            string testString = "test";
            Assert.AreEqual(1, Similarity.CalculateLevenshteinSimilarity(testString, testString).Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestLevenshteinDistanceWithEqualStrings()
        {
            string testString = "test";
            Assert.AreEqual(0, Similarity.CalculateLevenshteinDistance(testString, testString));
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestLevenshteinDistanceWithDifferentValues()
        {
            Assert.AreEqual(1, Similarity.CalculateLevenshteinDistance("test", "test2"));
            Assert.AreEqual(1, Similarity.CalculateLevenshteinDistance("test1", "test2"));
            Assert.AreEqual(4, Similarity.CalculateLevenshteinDistance("test", string.Empty));
            Assert.AreEqual(4, Similarity.CalculateLevenshteinDistance(string.Empty, "test"));
            Assert.AreEqual(4, Similarity.CalculateLevenshteinDistance("test", "a"));
            Assert.AreEqual(4, Similarity.CalculateLevenshteinDistance("a", "test"));
            Assert.AreEqual(3, Similarity.CalculateLevenshteinDistance("test", "aeaa"));
            Assert.AreEqual(3, Similarity.CalculateLevenshteinDistance("aeaa", "test"));
            Assert.AreEqual(4, Similarity.CalculateLevenshteinDistance("abcd", "efgh"));
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestLevenshteinSimilarityWithDifferentValues()
        {
            Assert.AreEqual((decimal)4 / 5, Similarity.CalculateLevenshteinSimilarity("test", "test2").Value);
            Assert.AreEqual((decimal)4 / 5, Similarity.CalculateLevenshteinSimilarity("test1", "test2").Value);
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity("test", string.Empty).Value);
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity(string.Empty, "test").Value);
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity("test", "a").Value);
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity("a", "test").Value);
            Assert.AreEqual((decimal)1 / 4, Similarity.CalculateLevenshteinSimilarity("test", "aeaa").Value);
            Assert.AreEqual((decimal)1 / 4, Similarity.CalculateLevenshteinSimilarity("aeaa", "test").Value);
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity("abcd", "efgh").Value);
            Assert.AreEqual((decimal)3 / 8, Similarity.CalculateLevenshteinSimilarity("xx345xxx", "yy345yyy").Value);
            Assert.AreEqual((decimal)5 / 8, Similarity.CalculateLevenshteinSimilarity("12xxx678", "12yyy678").Value);
            Assert.AreEqual((decimal)7 / 10, Similarity.CalculateLevenshteinSimilarity("12345678", "1x34567890").Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestLevenshteinSimilarityWithCompletelyDifferentStrings()
        {
            Assert.AreEqual(0, Similarity.CalculateLevenshteinSimilarity("abcd", "efgh").Value);
        }

        #endregion
        #region Cosine
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestCosineSimilarityWithEqualStrings()
        {
            string testString = "test";
            Assert.AreEqual(1, Similarity.CalculateCosineSimilarity(testString, testString).Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestCosineSimilarityWithCompletelyDifferentStrings()
        {
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity("abcd", "efgh").Value);
        }
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestCosineSimilarityWithDifferentValues()
        {
            this.TestRange(0.9.ToPercentValue(), Similarity.CalculateCosineSimilarity("test", "test2"), 1.ToPercentValue());
            this.TestRange(0.8.ToPercentValue(), Similarity.CalculateCosineSimilarity("test1", "test2"), 0.9.ToPercentValue());
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity("test", string.Empty).Value);
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity(string.Empty, "test").Value);
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity("test", "a").Value);
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity("a", "test").Value);
            this.TestRange(0.1.ToPercentValue(), Similarity.CalculateCosineSimilarity("test", "aeaa"), 0.2.ToPercentValue());
            this.TestRange(0.1.ToPercentValue(), Similarity.CalculateCosineSimilarity("aeaa", "test"), 0.2.ToPercentValue());
            Assert.AreEqual(0, Similarity.CalculateCosineSimilarity("abcd", "efgh").Value);
            this.TestRange(0.1.ToPercentValue(), Similarity.CalculateCosineSimilarity("xx345xxx", "yy345yyy"), 0.2.ToPercentValue());
            this.TestRange(0.3.ToPercentValue(), Similarity.CalculateCosineSimilarity("12xxx678", "12yyy678"), 0.4.ToPercentValue());
            this.TestRange(0.7.ToPercentValue(), Similarity.CalculateCosineSimilarity("12345678", "1x34567890"), 0.8.ToPercentValue());
        }

        #endregion
        #region Helper
        private void TestRange(PercentValue minimumValue, PercentValue actualValue, PercentValue maximalValue)
        {
            Assert.IsTrue(minimumValue < actualValue, $"Expected {minimumValue}<{actualValue}");
            Assert.IsTrue(actualValue < maximalValue, $"Expected {actualValue}<{maximalValue}");

            Assert.IsTrue(minimumValue.Value < actualValue.Value, $"Expected {minimumValue.Value}<{actualValue.Value}");
            Assert.IsTrue(actualValue.Value < maximalValue.Value, $"Expected {actualValue.Value}<{maximalValue.Value}");
        }
        #endregion
    }
}