using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class PercentValueTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DivisionByDoubleTest()
        {
            //arrange
            var value = new PercentValue(0.5);

            //act
            PercentValue result = value / 2.0;

            //assert
            Assert.AreEqual(0.25m, result.Value);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DivisionOfDoubleByPercentValueTest()
        {
            //arrange
            var value = new PercentValue(0.5);

            //act
            PercentValue result = 0.25 / value;

            //assert
            Assert.AreEqual(0.5m, result.Value);
        }
    }
}
