using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    [TestClass]
    public class DepthFirstSearchTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestSimpleSearch()
        {
            IList<Tuple<int, IList<Edge>>> order = [];
            Graph g = TestGraphs.GetTestGraphWithoutLoop();
            g.DepthFirstSearch((v, edges) =>
            {
                order.Add(new Tuple<int, IList<Edge>>(int.Parse(v.Name.Replace("v", string.Empty)), edges));
                return true;
            });
            Assert.HasCount(10, order);
            int i1 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 1);
            int i2 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 2);
            int i3 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 3);
            int i4 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 4);
            int i5 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 5);
            int i6 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 6);
            int i7 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 7);
            int i8 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 8);
            int i9 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 9);
            int i10 = this.GetIndexOfTupleWithSpeicficFirstvalue(order, 10);
            Assert.AreEqual(0, i1);
            Assert.IsLessThan(i4, i1);
            Assert.IsLessThan(i8, i4);
            Assert.IsLessThan(i3, i1);
            Assert.IsLessThan(i7, i3);
            Assert.IsLessThan(i6, i3);
            Assert.IsLessThan(i10, i6);
            Assert.IsLessThan(i2, i1);
            Assert.IsLessThan(i5, i2);
            Assert.IsLessThan(i9, i5);
        }

        private int GetIndexOfTupleWithSpeicficFirstvalue(IList<Tuple<int, IList<Edge>>> l, int index)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Item1.Equals(index))
                {
                    return i;
                }
            }
            throw new KeyNotFoundException();
        }
    }
}