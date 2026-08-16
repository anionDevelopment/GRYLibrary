using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class GRYDateTimeTests
    {
        [TestMethod]
        public void TestGRYDateTimeFromString()
        {
            // arrange
            string input = "2017-12-14 11:55:54";
            GRYDateTime expected = new GRYDateTime(2017, 12, 14, 11, 55, 54);

            // act
            GRYDateTime actual = GRYDateTime.FromString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}
