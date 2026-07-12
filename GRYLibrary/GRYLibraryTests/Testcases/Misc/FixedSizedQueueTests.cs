using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class FixedSizedQueueTests
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void FixedSizedQueueTest()
        {
            //arrange
            var queue = new FixedSizeQueue<int>(3);

            //act & assert
            queue.Enqueue(1);
            Assert.IsTrue(new int[] { 1 }.SequenceEqual(queue.GetEntries()));
            queue.Enqueue(2);
            Assert.IsTrue(new int[] { 1,2 }.SequenceEqual(queue.GetEntries()));
            queue.Enqueue(3);
            Assert.IsTrue(new int[] { 1,2,3 }.SequenceEqual(queue.GetEntries()));
            queue.Enqueue(4);
            Assert.IsTrue(new int[] { 2,3,4 }.SequenceEqual(queue.GetEntries()));
        }
    }
}
