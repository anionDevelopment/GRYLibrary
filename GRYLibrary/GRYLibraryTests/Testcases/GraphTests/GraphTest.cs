using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    /// <summary>
    /// Contains simple graph tests
    /// </summary>
    [TestClass]
    public class GraphTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SimpleVertexTest()
        {
            Vertex v1 = new("v1");
            Assert.AreEqual(0, v1.GetConnectedEdges().Count);
            Assert.AreEqual("v1", v1.Name);
            Assert.AreEqual(v1, v1);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void VertexEqual()
        {
            Vertex v1 = new("v");
            Vertex v2 = new("v");
            Assert.AreEqual(v1, v2);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SimpleEdgeTest()
        {
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            DirectedEdge e1 = new(v1, v2, "e", 1.5);
            Assert.AreEqual(v1, e1.Source);
            Assert.AreEqual(v2, e1.Target);
            Assert.AreEqual("e", e1.Name);
            Assert.AreEqual(1.5, e1.Weight);
            DirectedEdge e2 = new(v1, v2, "e", 1.5);
            Assert.AreEqual(e1, e2);
            DirectedEdge e3 = new(v1, v2, "e", 1.6);
            Assert.AreNotEqual(e1, e3);
            DirectedEdge e4 = new(v2, v1, "e", 1.5);
            Assert.AreNotEqual(e1, e4);
            DirectedEdge e5 = new(v1, new Vertex("v3"), "e", 1.5);
            Assert.AreNotEqual(e1, e5);

        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void EdgeEquals()
        {
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            DirectedEdge e1 = new(v1, v2, "e1");
            DirectedEdge e21 = new(v1, v2, "e2");
            DirectedEdge e22 = new(v2, v1, "e3");
            Assert.AreEqual(e1, e1);
            Assert.AreNotEqual(e1, e21);
            Assert.AreNotEqual(e1, e22);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SimpleDirectedGraphTest()
        {
            DirectedGraph g = new();
            Assert.IsTrue(g.SelfLoopIsAllowed);
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            Vertex v3 = new("v3");
            Vertex v4 = new("v4");
            Vertex v5 = new("v5");

            DirectedEdge e1 = new(v1, v2, "e1");
            DirectedEdge e2 = new(v2, v3, "e2");
            DirectedEdge e3 = new(v3, v4, "e3");
            DirectedEdge e4 = new(v4, v5, "e4");
            DirectedEdge e5 = new(v5, v1, "e5");

            g.AddVertex(v1);
            g.AddVertex(v2);
            g.AddVertex(v3);
            g.AddVertex(v4);
            g.AddVertex(v5);
            g.AddEdge(e1);
            g.AddEdge(e2);
            g.AddEdge(e3);
            g.AddEdge(e4);
            g.AddEdge(e5);

            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(5, g.Edges.Count);
            Assert.AreEqual(2, v1.GetConnectedEdges().Count);

            //test TryGetConnectionBetween:
            Assert.IsTrue(g.TryGetEdge(v1, v2, out Edge e1Reloaded));
            Assert.AreEqual(e1, e1Reloaded);

            DirectedEdge e45 = new(v4, v5, "e45");
            try
            {
                g.AddEdge(e45);//edge this this source and target does already exist
                Assert.Fail();
            }
            catch
            {
            }

            //test TryGetConnectionBetween with selfloop:
            DirectedEdge eSelfLoop = new(v1, v1, "e11");
            g.AddEdge(eSelfLoop);
            g.TryGetEdge(v1, v1, out Edge eSelfLoopReloaded);
            Assert.AreEqual(eSelfLoop, eSelfLoopReloaded);
            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(6, g.Edges.Count);
            Assert.AreEqual(2, g.GetMinimumDegree());
            Assert.AreEqual(4, g.GetMaximumDegree());

            try
            {
                g.SelfLoopIsAllowed = false;//g does already have a selfloop
                Assert.Fail();
            }
            catch
            {
            }
            Assert.IsTrue(g.SelfLoopIsAllowed);

            g.RemoveEdge(eSelfLoop);
            Assert.IsFalse(g.TryGetEdge(v1, v1, out _));
            g.SelfLoopIsAllowed = false;
            Assert.IsFalse(g.SelfLoopIsAllowed);
            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(5, g.Edges.Count);

            ISet<Vertex> successorsOfv5 = g.GetDirectSuccessors(v5);
            Assert.AreEqual(1, successorsOfv5.Count);
            Assert.AreEqual(v1, successorsOfv5.First());

            DirectedEdge e43 = new(v4, v3, "e43");
            g.AddEdge(e43);
            ISet<Vertex> successorsOfv4 = g.GetDirectSuccessors(v4);
            Assert.AreEqual(2, successorsOfv4.Count);
            Assert.IsTrue(new HashSet<Vertex>() { v3, v5 }.SetEquals(successorsOfv4));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SimpleUndrectedGraphTest()
        {
            UndirectedGraph g = new();
            Assert.IsTrue(g.SelfLoopIsAllowed);
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            Vertex v3 = new("v3");
            Vertex v4 = new("v4");
            Vertex v5 = new("v5");

            UndirectedEdge e1 = new(new Vertex[] { v1, v2 }, "e1");
            UndirectedEdge e2 = new(new Vertex[] { v2, v3 }, "e2");
            UndirectedEdge e3 = new(new Vertex[] { v3, v4 }, "e3");
            UndirectedEdge e4 = new(new Vertex[] { v4, v5 }, "e4");
            UndirectedEdge e5 = new(new Vertex[] { v5, v1 }, "e5");

            g.AddVertex(v1);
            g.AddVertex(v2);
            g.AddVertex(v3);
            g.AddVertex(v4);
            g.AddVertex(v5);
            g.AddEdge(e1);
            g.AddEdge(e2);
            g.AddEdge(e3);
            g.AddEdge(e4);
            g.AddEdge(e5);

            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(5, g.Edges.Count);
            Assert.AreEqual(2, v1.GetConnectedEdges().Count);

            //test TryGetEdge:
            Assert.IsTrue(g.TryGetEdge(v1, v2, out Edge e1Reloaded1));
            Assert.AreEqual(e1, e1Reloaded1);
            Assert.IsTrue(g.TryGetEdge(v2, v1, out Edge e1Reloaded2));
            Assert.AreEqual(e1, e1Reloaded2);

            Assert.AreEqual(e1Reloaded1, e1Reloaded2);

            UndirectedEdge e54 = new(new Vertex[] { v5, v4 }, "e54");
            try
            {
                g.AddEdge(e54);//edge this this source and target does already exist
                Assert.Fail();
            }
            catch
            {
            }

            //test TryGetConnectionBetween with selfloop:
            UndirectedEdge eSelfLoop = new(new Vertex[] { v1, v1 }, "e11");
            g.AddEdge(eSelfLoop);
            g.TryGetEdge(v1, v1, out Edge eSelfLoopReloaded);
            Assert.AreEqual(eSelfLoop, eSelfLoopReloaded);
            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(6, g.Edges.Count);

            try
            {
                g.SelfLoopIsAllowed = false;//g does already have a selfloop
                Assert.Fail();
            }
            catch
            {
            }
            Assert.IsTrue(g.SelfLoopIsAllowed);

            g.RemoveEdge(eSelfLoop);
            Assert.IsFalse(g.TryGetEdge(v1, v1, out _));
            g.SelfLoopIsAllowed = false;
            Assert.IsFalse(g.SelfLoopIsAllowed);
            Assert.AreEqual(5, g.Vertices.Count);
            Assert.AreEqual(5, g.Edges.Count);

            ISet<Vertex> successorsOfv4 = g.GetDirectSuccessors(v4, true);
            Assert.AreEqual(2, successorsOfv4.Count);
            Assert.IsTrue(new HashSet<Vertex>() { v3, v5 }.SetEquals(successorsOfv4));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GraphTpAdjacencyMatrixTest()
        {
            DirectedGraph graph = this.GetTestGraph();
            double[,] adjacencyMatrix = this.GetTestAdjacencyMatrix();
            double[,] toAdjacencyMatrixResult = graph.ToAdjacencyMatrix();
            Assert.IsTrue(Core.Misc.Utilities.TwoDimensionalArrayEquals(adjacencyMatrix, toAdjacencyMatrixResult), $"Expected {Core.Misc.Utilities.TwoDimensionalArrayToString(adjacencyMatrix)} but was {Core.Misc.Utilities.TwoDimensionalArrayToString(toAdjacencyMatrixResult)}");
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AdjacencyMatrixToGraphTest()
        {
            DirectedGraph graph = this.GetTestGraph();
            DirectedGraph createByAdjacencyMatrix = DirectedGraph.CreateByAdjacencyMatrix(this.GetTestAdjacencyMatrix());
            Assert.AreEqual(graph, createByAdjacencyMatrix);
        }
        private double[,] GetTestAdjacencyMatrix()
        {
            double[,] adjacencyMatrix = new double[4, 4];
            adjacencyMatrix[0, 0] = 1;
            adjacencyMatrix[0, 1] = 0.8;
            adjacencyMatrix[0, 2] = 0;
            adjacencyMatrix[0, 3] = 0;
            adjacencyMatrix[1, 0] = 0;
            adjacencyMatrix[1, 1] = 0;
            adjacencyMatrix[1, 2] = 1;
            adjacencyMatrix[1, 3] = 1;
            adjacencyMatrix[2, 0] = 0.2;
            adjacencyMatrix[2, 1] = 0;
            adjacencyMatrix[2, 2] = 0;
            adjacencyMatrix[2, 3] = 1;
            adjacencyMatrix[3, 0] = 0;
            adjacencyMatrix[3, 1] = 0;
            adjacencyMatrix[3, 2] = 0;
            adjacencyMatrix[3, 3] = 0;
            return adjacencyMatrix;
        }

        private DirectedGraph GetTestGraph()
        {
            DirectedGraph graph = new();
            Vertex v0 = new("Vertex_1");
            Vertex v1 = new("Vertex_2");
            Vertex v2 = new("Vertex_3");
            Vertex v3 = new("Vertex_4");
            graph.AddEdge(new DirectedEdge(v0, v0, "e1"));
            graph.AddEdge(new DirectedEdge(v0, v1, "e2", 0.8));
            graph.AddEdge(new DirectedEdge(v1, v2, "e3"));
            graph.AddEdge(new DirectedEdge(v1, v3, "e4"));
            graph.AddEdge(new DirectedEdge(v2, v0, "e5", 0.2));
            graph.AddEdge(new DirectedEdge(v2, v3, "e6"));
            return graph;
        }
    }
}