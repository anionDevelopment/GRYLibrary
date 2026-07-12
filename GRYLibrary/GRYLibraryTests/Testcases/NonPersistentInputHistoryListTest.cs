using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class NonPersistentInputHistoryListTest
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void NonPersistentInputHistoryListTest1()
        {
            NonPersistentInputHistoryList inpustList = new();

            string input1 = "1";
            string input2 = "2";
            string input3 = "3";
            string input4 = "4";
            string input5 = "5";

            Assert.AreEqual(string.Empty, inpustList.DownPressed());
            Assert.AreEqual(string.Empty, inpustList.UpPressed());

            inpustList.EnterPressed(input1);

            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());

            inpustList.EnterPressed(input2);

            Assert.AreEqual(input2, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.DownPressed());

            inpustList.EnterPressed(input3);
            inpustList.EnterPressed(input4);

            Assert.AreEqual(input4, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.DownPressed());
            Assert.AreEqual(input3, inpustList.DownPressed());
            Assert.AreEqual(input4, inpustList.DownPressed());
            Assert.AreEqual(string.Empty, inpustList.DownPressed());
            Assert.AreEqual(string.Empty, inpustList.DownPressed());

            Assert.AreEqual(input4, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.UpPressed());

            inpustList.EnterPressed(input5);

            Assert.AreEqual(input5, inpustList.UpPressed());
            Assert.AreEqual(input4, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input1, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.DownPressed());
            Assert.AreEqual(input3, inpustList.DownPressed());
            Assert.AreEqual(input4, inpustList.DownPressed());
            Assert.AreEqual(input5, inpustList.DownPressed());
            Assert.AreEqual(string.Empty, inpustList.DownPressed());
            Assert.AreEqual(input5, inpustList.UpPressed());
            Assert.AreEqual(input4, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.UpPressed());
            Assert.AreEqual(input2, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.DownPressed());
            Assert.AreEqual(input2, inpustList.UpPressed());
            Assert.AreEqual(input3, inpustList.DownPressed());

        }
    }
}