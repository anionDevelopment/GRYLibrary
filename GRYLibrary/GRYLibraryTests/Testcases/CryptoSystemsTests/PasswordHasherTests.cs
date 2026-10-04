using GRYLibrary.Core.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.CryptoSystemsTests
{
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void VerifyAcceptsTheCorrectPassword()
        {
            // arrange
            PasswordHasher hasher = new PasswordHasher(10000);
            string hash = hasher.Hash("correct horse battery staple");

            // act & assert
            Assert.IsTrue(hasher.Verify("correct horse battery staple", hash));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void VerifyRejectsAWrongPassword()
        {
            // arrange
            PasswordHasher hasher = new PasswordHasher(10000);
            string hash = hasher.Hash("correct horse battery staple");

            // act & assert
            Assert.IsFalse(hasher.Verify("wrong password", hash));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void HashingTheSamePasswordTwiceYieldsDifferentHashesBecauseOfTheSalt()
        {
            // arrange
            PasswordHasher hasher = new PasswordHasher(10000);

            // act
            string firstHash = hasher.Hash("same password");
            string secondHash = hasher.Hash("same password");

            // assert
            Assert.AreNotEqual(firstHash, secondHash);
            Assert.IsTrue(hasher.Verify("same password", firstHash));
            Assert.IsTrue(hasher.Verify("same password", secondHash));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void VerifyReturnsFalseForAMalformedHashInsteadOfThrowing()
        {
            // arrange
            PasswordHasher hasher = new PasswordHasher(10000);

            // act & assert
            Assert.IsFalse(hasher.Verify("whatever", "not-a-valid-hash-string"));
        }
    }
}
