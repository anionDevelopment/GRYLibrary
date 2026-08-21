using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class UnorderedListTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnorderedList_True_1()
        {
            // arrange 
            UnorderedList<int> list1 = [1, 2, 2, 3];
            UnorderedList<int> list2 = [2, 3, 2, 1];
            bool expected = true;

            // act
            bool actual = list1.Equals(list2);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnorderedList_True_2()
        {
            // arrange 
            UnorderedList<int> list1 = [];
            UnorderedList<int> list2 = [];
            bool expected = true;

            // act
            bool actual = list1.Equals(list2);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnorderedList_False_1()
        {
            // arrange 
            UnorderedList<int> list1 = [1, 2, 2, 3];
            UnorderedList<int> list2 = [2, 3, 2, 1, 2];
            bool expected = false;

            // act
            bool actual = list1.Equals(list2);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnorderedList_False_2()
        {
            // arrange 
            UnorderedList<int> list1 = [1, 2, 3];
            UnorderedList<int> list2 = [2, 3, 1, 4];
            bool expected = false;

            // act
            bool actual = list1.Equals(list2);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnorderedList_False_3()
        {
            // arrange 
            UnorderedList<int> list1 = [1, 2, 3, 4];
            UnorderedList<int> list2 = [2, 3, 1];
            bool expected = false;

            // act
            bool actual = list1.Equals(list2);

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}
