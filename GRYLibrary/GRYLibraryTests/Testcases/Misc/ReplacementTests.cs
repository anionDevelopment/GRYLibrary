using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.Miscellaneous
{
    [TestClass]
    public class ReplacementTests
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void TestReplaceBooleanVariable()
        {
            Assert.AreEqual("a", ReplacementTools.ReplaceBooleanVariable("a", "b", true));
            Assert.AreEqual("__[a]__b__[/a]__", ReplacementTools.ReplaceBooleanVariable("__[a]__b__[/a]__", "b", true));
            Assert.AreEqual("bc", ReplacementTools.ReplaceBooleanVariable("__[a]__b__[/a]__c", "a", true));
            Assert.AreEqual("c", ReplacementTools.ReplaceBooleanVariable("__[a]__b__[/a]__c", "a", false));
            Assert.AreEqual("bcde", ReplacementTools.ReplaceBooleanVariable("__[a]__b__[/a]__c__[a]__d__[/a]__e", "a", true));
            Assert.AreEqual("ce", ReplacementTools.ReplaceBooleanVariable("__[a]__b__[/a]__c__[a]__d__[/a]__e", "a", false));
        }
    }
}