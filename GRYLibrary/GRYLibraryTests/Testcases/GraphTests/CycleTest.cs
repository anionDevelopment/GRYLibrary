using GRYLibrary.Core.Graph;
using GRYLibrary.Core.Graph.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    [TestClass]
    public class CycleTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            Vertex v1 = new(nameof(v1));
            Vertex v2 = new(nameof(v2));
            Vertex v3 = new(nameof(v3));
            Vertex v4 = new(nameof(v4));
            DirectedEdge edge1 = new(v3, v4, "e1");
            DirectedEdge edge2 = new(v4, v1, "e2");
            DirectedEdge edge3 = new(v1, v2, "e3");
            DirectedEdge edge4 = new(v2, v3, "e4");

            List<Edge> cycleItems = [edge3, edge4, edge1, edge2];

            Cycle cycle = new(cycleItems);

            List<DirectedEdge> cycleInternalOrder = [edge3, edge4, edge1, edge2];

            Assert.IsTrue(cycle.Edges.SequenceEqual(cycleInternalOrder));
        }
        [TestMethod]
        public void TestRepresentsCycle()
        {
            Vertex v1 = new(nameof(v1));
            Vertex v2 = new(nameof(v2));
            Vertex v3 = new(nameof(v3));
            Vertex v4 = new(nameof(v3));
            DirectedEdge edge1 = new(v1, v2, "e1");
            DirectedEdge edge2 = new(v2, v3, "e2");
            DirectedEdge edge3 = new(v3, v1, "e3");
            DirectedEdge edge4 = new(v3, v4, "e4");
            DirectedEdge edge5 = new(v4, v3, "e5");

            List<Edge> cycleItems = [edge1];
            Assert.IsFalse(Cycle.RepresentsCycle(cycleItems));

            cycleItems.Add(edge2);
            Assert.IsFalse(Cycle.RepresentsCycle(cycleItems));

            cycleItems.Add(edge3);
            Assert.IsTrue(Cycle.RepresentsCycle(cycleItems));

            cycleItems.Add(edge4);
            Assert.IsFalse(Cycle.RepresentsCycle(cycleItems));

            cycleItems.Add(edge5);
            Assert.IsFalse(Cycle.RepresentsCycle(cycleItems));

        }
        [TestMethod]
        public void TestErrorsOfCycleConstructorsDueToEmptyEdgesList()
        {
            Assert.Throws<InvalidGraphStructureException>(() => new Cycle(Array.Empty<Edge>()));
        }
        [TestMethod]
        public void TestErrorsOfCycleConstructorsDueToUncyclicEdges()
        {
            Vertex v1 = new(nameof(v1));
            Vertex v2 = new(nameof(v2));
            Vertex v3 = new(nameof(v3));
            DirectedEdge edge1 = new(v1, v2, "e1");
            DirectedEdge edge2 = new(v2, v3, "e2");
            Assert.Throws<InvalidGraphStructureException>(() => new Cycle(new Edge[] { edge1, edge2 }));
        }
        [TestMethod]
        public void TestErrorsOfCycleConstructorsDueToDuplicatedEdges()
        {
            Vertex v1 = new(nameof(v1));
            Vertex v2 = new(nameof(v2));
            Vertex v3 = new(nameof(v3));
            DirectedEdge edge1 = new(v1, v2, "e1");
            DirectedEdge edge2 = new(v2, v3, "e2");
            DirectedEdge edge3 = new(v3, v1, "e3");
            Assert.Throws<InvalidGraphStructureException>(() => new Cycle(new Edge[] { edge1, edge2, edge3, edge1, edge2, edge3 }));
        }
    }
}