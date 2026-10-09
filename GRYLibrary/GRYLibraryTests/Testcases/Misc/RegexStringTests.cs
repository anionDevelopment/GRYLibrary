using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class RegexStringTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AssigningMatchingValueSetsValue()
        {
            //arrange
            RegexString regexString = new RegexString();
            regexString.SetRegex(new Regex("^[a-z]+$"));

            //act
            regexString.Value = "abc";

            //assert
            Assert.AreEqual("abc", regexString.Value);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AssigningNotMatchingValueThrowsAndKeepsPreviousValue()
        {
            //arrange
            RegexString regexString = new RegexString();
            regexString.SetRegex(new Regex("^[a-z]+$"));
            regexString.Value = "abc";

            //act
            ArgumentException exception = Assert.Throws<ArgumentException>(() => regexString.Value = "ABC");

            //assert
            Assert.AreEqual("abc", regexString.Value);
            StringAssert.Contains(exception.Message, "\"ABC\"");
        }
    }
}
