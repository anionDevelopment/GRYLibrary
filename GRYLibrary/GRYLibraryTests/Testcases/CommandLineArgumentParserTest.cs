using GRYLibrary.Core.ExecutePrograms.Detached;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class CommandLineArgumentParserTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseReturnsAnEmptyListForAnEmptyString()
        {
            IList<string> result = CommandLineArgumentParser.Parse(string.Empty);
            Assert.IsEmpty(result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseReturnsAnEmptyListForNull()
        {
            IList<string> result = CommandLineArgumentParser.Parse(null);
            Assert.IsEmpty(result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseSplitsOnWhitespace()
        {
            IList<string> result = CommandLineArgumentParser.Parse("one two three");
            CollectionAssert.AreEqual(new List<string>() { "one", "two", "three" }, new List<string>(result));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseKeepsQuotedWhitespaceTogether()
        {
            IList<string> result = CommandLineArgumentParser.Parse("\"with space\" plain");
            CollectionAssert.AreEqual(new List<string>() { "with space", "plain" }, new List<string>(result));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseKeepsASingleBackslashWhichIsNotFollowedByAQuoteLiteral()
        {
            // The argument-string contains "a\b".
            IList<string> result = CommandLineArgumentParser.Parse("a\\b");
            CollectionAssert.AreEqual(new List<string>() { "a\\b" }, new List<string>(result));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseTreatsABackslashQuoteAsAnEscapedQuote()
        {
            // The argument-string contains "a\"b" (a, backslash, quote, b, closing quote is not present),
            // represented here inside quotes so the whole token stays together: "a\"b".
            IList<string> result = CommandLineArgumentParser.Parse("\"a\\\"b\"");
            CollectionAssert.AreEqual(new List<string>() { "a\"b" }, new List<string>(result));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseTreatsTwoBackslashesBeforeAQuoteAsOneLiteralBackslashAndAQuoteDelimiter()
        {
            // The argument-string contains: \\"a b" -> one literal backslash, then a quoted "a b".
            IList<string> result = CommandLineArgumentParser.Parse("\\\\\"a b\"");
            CollectionAssert.AreEqual(new List<string>() { "\\a b" }, new List<string>(result));
        }
    }
}
