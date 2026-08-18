using GRYLibrary.Core.Misc;
using GRYLibrary.Core.AOA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.AdvancedObjectAnalysisTests.Equals
{
    [TestClass]
    public class PrimitiveComparerTest
    {

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PrimitiveEqualsTestString()
        {
            string testString = "test";
            PropertyEqualsCalculator comparer = new();
            Assert.IsTrue(comparer.Equals(testString, testString));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PrimitiveEqualsTestInt()
        {
            int testInt = 4;
            PropertyEqualsCalculator comparer = new();
            Assert.IsTrue(comparer.Equals(testInt, testInt));
        }
    }
}