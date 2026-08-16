using GRYLibrary.Core.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GRYLibrary.Core.Misc.Utilities;
using static GRYLibrary.Tests.Utilities.TestUtilities;

namespace GRYLibrary.Tests.Testcases.CryptoSystemsTests
{
    [TestClass]
    public class SHA256PureCSharpTests
    {
        [TestMethod]
        public void SHA256PureTests()
        {
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(string.Empty);
            AssertPureSHA256ValueIsEqualsToDotNetImplementation("Franz jagt im komplett verwahrlosten Taxi quer durch Bayern");
            AssertPureSHA256ValueIsEqualsToDotNetImplementation("Simple ASCII input");
            AssertPureSHA256ValueIsEqualsToDotNetImplementation("Long input test test test test test test test test test test test test test test test test test test test test test test test test test test test test test");
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(SpecialCharacterTestString);
        }

        /// <remarks>
        /// A message whose length is 55 bytes (or 55+64*n bytes) is exactly as long that no additional zero-bits have to be appended while padding.
        /// This is the edge-case of the padding-calculation and therefore tested explicitly.
        /// </remarks>
        [TestMethod]
        public void SHA256PureTestsWithInputWhichDoesNotRequireAdditionalPadding()
        {
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(new string('a', 55));
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(new string('a', 119));
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(new string('a', 54));
            AssertPureSHA256ValueIsEqualsToDotNetImplementation(new string('a', 56));
        }

        [TestMethod]
        public void RightRotateTests()
        {
            Assert.AreEqual("10110100010111010101010100010010", UintToBinaryString(SHA256PureCSharp.RightRotate(BinaryStringToUint("10001011101010101010001001010110"), 5)));
        }
    }
}