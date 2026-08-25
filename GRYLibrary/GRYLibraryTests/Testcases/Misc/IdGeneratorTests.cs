using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class IdGeneratorTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IntIdGeneratorGeneratesIncreasingIds()
        {
            // arrange
            IdGenerator<int> idGenerator = IdGenerator.GetDefaultIntIdGenerator();

            // act & assert
            Assert.AreEqual(1, idGenerator.GenerateNewId());
            Assert.AreEqual(2, idGenerator.GenerateNewId());
            Assert.AreEqual(3, idGenerator.GenerateNewId());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GeneratedIdsContainsAllGeneratedIds()
        {
            // arrange
            IdGenerator<int> idGenerator = IdGenerator.GetDefaultIntIdGenerator();

            // act
            idGenerator.GenerateNewId();
            idGenerator.GenerateNewId();
            idGenerator.GenerateNewId();

            // assert
            Assert.IsTrue(new HashSet<int>() { 1, 2, 3 }.SetEquals(idGenerator.GeneratedIds()));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ResetSetsTheGeneratorBackToItsInitialState()
        {
            // arrange
            IdGenerator<int> idGenerator = IdGenerator.GetDefaultIntIdGenerator();
            idGenerator.GenerateNewId();
            idGenerator.GenerateNewId();

            // act
            idGenerator.Reset();

            // assert
            Assert.AreEqual(1, idGenerator.GenerateNewId());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ResetWithLastValueContinuesAtTheGivenValue()
        {
            // arrange
            IdGenerator<int> idGenerator = IdGenerator.GetDefaultIntIdGenerator();

            // act
            idGenerator.Reset(41);

            // assert
            Assert.AreEqual(42, idGenerator.GenerateNewId());
        }
    }
}
