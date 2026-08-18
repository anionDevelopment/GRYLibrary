using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.GraphTests
{

    [TestClass]
    public class BreadthFirstSearchTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestSimpleBreadthFirstSearch()
        {
            List<int> order = [];
            Graph g = TestGraphs.GetTestGraphWithoutLoop();
            g.BreadthFirstSearch((v, edges) =>
            {
                order.Add(int.Parse(v.Name.Replace("v", string.Empty)));
                return true;
            });
            Assert.AreEqual(10, order.Count);
            Assert.AreEqual(1, order[0]);
            Assert.IsTrue(new HashSet<int>(new int[] { 2, 3, 4 }).SetEquals(new int[] { order[1], order[2], order[3] }));
            Assert.IsTrue(new HashSet<int>(new int[] { 5, 6, 7, 8 }).SetEquals(new int[] { order[4], order[5], order[6], order[7] }));
            Assert.IsTrue(new HashSet<int>(new int[] { 9, 10 }).SetEquals(new int[] { order[8], order[9] }));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestSimpleBreadthFirstSearch2()
        {
            List<Tuple<Vertex, IList<Edge>>> order = [];
            Graph g = TestGraphs.GetTestGraphWithoutLoop();
            g.BreadthFirstSearch((v, edges) =>
            {
                order.Add(new Tuple<Vertex, IList<Edge>>(v, edges));
                return true;
            });
            Assert.AreEqual(g.GetVertex("v1"), order[0].Item1);
            Assert.AreEqual(0, order[0].Item2.Count);
            //todo write asserts for content of order of edges
        }
    }
}