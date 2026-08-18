using GRYLibrary.Core.Misc;
using GRYLibrary.Core.AOA;
using GRYLibrary.Tests.TestData.TestTypes.CyclicDataStructure;
using GRYLibrary.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GRYLibrary.Tests.Testcases.AdvancedObjectAnalysisTests.Equals
{
    [TestClass]
    public class EqualsTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void NotEqualCyclicTestObject1()
        {
            CycleA testObject1 = CycleA.GetRandom();
            CycleA testObject2 = CycleA.GetRandom();
            TestUtilities.AssertNotEqual(testObject1, testObject2);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void NotEqualCyclicTestObject2()
        {
            CycleA testObject1 = CycleA.GetRandom();
            CycleA testObject2 = Core.Misc.Utilities.DeepClone(testObject1);
            testObject2.B.Id = Guid.NewGuid();
            TestUtilities.AssertNotEqual(testObject1, testObject2);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void EqualCyclicTestObject()
        {
            CycleA testObject1 = CycleA.GetRandom();
            CycleA testObject2 = Core.Misc.Utilities.DeepClone(testObject1);
            TestUtilities.AssertEqual(testObject1, testObject2);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PrimitiveEqualsTestObjectWithSameObject()
        {
            object testObject = new();
            PropertyEqualsCalculator comparer = new();
            Assert.IsTrue(comparer.Equals(testObject, testObject));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PrimitiveEqualsTestObjectWithEqualObject()
        {
            PropertyEqualsCalculator comparer = new();
            Assert.IsTrue(comparer.Equals(new object(), new object()));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PrimitiveEqualsTestObjectWithTypes()
        {
            PropertyEqualsCalculator comparer = new();
            Assert.IsTrue(comparer.Equals(typeof(object), typeof(object)));
            Assert.IsTrue(comparer.Equals(typeof(int), typeof(int)));
            Assert.IsTrue(comparer.Equals(typeof(Func<List<ISet<Uri>>>), typeof(Func<List<ISet<Uri>>>)));
            Assert.IsTrue(comparer.Equals(typeof(IList<string>), typeof(IList<string>)));
            Assert.IsFalse(comparer.Equals(typeof(IList<string>), typeof(List<string>)));
            Assert.IsFalse(comparer.Equals(typeof(List<string>), typeof(IList<string>)));
            Assert.IsTrue(comparer.Equals(typeof(List<string>), typeof(List<string>)));
            Assert.IsFalse(comparer.Equals(typeof(Func<List<ISet<Uri>>>), typeof(Func<List<HashSet<Uri>>>)));
            Assert.IsTrue(comparer.Equals(typeof(Uri), typeof(Uri)));
            Assert.IsFalse(comparer.Equals(typeof(Uri), typeof(Encoding)));
            Assert.IsTrue(comparer.Equals(typeof(Action<IList<string>>), typeof(Action<IList<string>>)));
            Assert.IsFalse(comparer.Equals(typeof(Action<IList<string>>), typeof(Action<List<string>>)));
            Assert.IsFalse(comparer.Equals(typeof(Action<List<string>>), typeof(Action<IList<string>>)));
            Assert.IsTrue(comparer.Equals(typeof(Action<List<string>>), typeof(Action<List<string>>)));
        }
    }
}