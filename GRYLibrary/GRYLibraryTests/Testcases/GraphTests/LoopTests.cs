using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    /// <summary>
    /// Contains complex graph tests
    /// </summary>
    [TestClass]
    public class LoopTests
    {
        [TestMethod]
        public void HasOneOrMoreCycleTest1()
        {
            DirectedGraph graph = TestGraphs.GetTestConnectedGraphWithSimpleLoop();
            Assert.IsTrue(graph.ContainsOneOrMoreCycles());
            Assert.IsFalse(graph.ContainsOneOrMoreSelfLoops());
            Assert.IsTrue(graph.HasHamiltonianCycle(out _));
            ISet<Cycle> cycles = graph.GetAllCycles();
            Assert.AreEqual(1, cycles.Count);
        }
        [TestMethod]
        public void HasOneOrMoreCycleTest()
        {
            DirectedGraph graph = TestGraphs.GetTestGraphWithoutLoop2();
            Assert.IsFalse(graph.ContainsOneOrMoreCycles());
            Assert.IsFalse(graph.ContainsOneOrMoreSelfLoops());
        }
        [TestMethod]
        public void IsConnectedTest1()
        {
            Assert.IsTrue(TestGraphs.GetTestConnectedGraphWithSimpleLoop().ToUndirectedGraph().IsConnected());
            Assert.IsTrue(TestGraphs.GetTestConnectedGraphWithSimpleLoop().IsConnected());
        }
    }
}