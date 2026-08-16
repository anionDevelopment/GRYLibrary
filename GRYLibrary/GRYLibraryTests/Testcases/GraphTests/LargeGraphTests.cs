using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    [TestClass]
    public class LargeGraphTests
    {
        public readonly UndirectedGraph NotDirectedConnectedCyclicGraph = GetNotDirectedConnectedCyclicGraph();
        public readonly UndirectedGraph NotDirectedConnectedNotCyclicGraph = GetNotDirectedConnectedNotCyclicGraph();
        public readonly UndirectedGraph NotDirectedNotConnectedCyclicGraph = GetNotDirectedNotConnectedCyclicGraph();
        public readonly UndirectedGraph NotDirectedNotConnectedNotCyclicGraph = GetNotDirectedNotConnectedNotCyclicGraph();
        public readonly DirectedGraph DirectedConnectedCyclicGraph = GetDirectedConnectedCyclicGraph();
        public readonly DirectedGraph DirectedConnectedNotCyclicGraph = GetDirectedConnectedNotCyclicGraph();
        public readonly DirectedGraph DirectedNotConnectedCyclicGraph = GetDirectedNotConnectedCyclicGraph();
        public readonly DirectedGraph DirectedNotConnectedNotCyclicGraph = GetDirectedNotConnectedNotCyclicGraph();

        private static UndirectedGraph CreateUndirectedGraph(IEnumerable<DirectedEdge> bigStructure)
        {
            UndirectedGraph result = new();
            foreach (DirectedEdge edge in bigStructure)
            {
                result.AddEdge(edge);
            }
            return result;
        }

        private static void MakeUnconnected(IList<DirectedEdge> existingStructure)
        {
            foreach (DirectedEdge item in CreateRandomConnectedNotCyclicEdgesWithNewVertices())
            {
                existingStructure.Add(item);
            }
        }

        private static IEnumerable<DirectedEdge> CreateRandomConnectedNotCyclicEdgesWithNewVertices()
        {
            throw new NotImplementedException();
        }

        private static IList<DirectedEdge> GetLargeUndirectedConnectedNotCyclicGraphStructure()
        {
            throw new NotImplementedException();
        }

        private static void AddCycle(IList<DirectedEdge> existingStructure)
        {
            throw new NotImplementedException();
        }
        private static DirectedGraph CreateDirectedGraph(IEnumerable<DirectedEdge> bigStructure)
        {
            DirectedGraph result = new();
            foreach (DirectedEdge edge in bigStructure)
            {
                result.AddEdge(edge);
            }
            return result;
        }

        private static UndirectedGraph GetNotDirectedConnectedCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            AddCycle(bigStructure);
            return CreateUndirectedGraph(bigStructure);
        }

        private static UndirectedGraph GetNotDirectedConnectedNotCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            return CreateUndirectedGraph(bigStructure);
        }

        private static UndirectedGraph GetNotDirectedNotConnectedCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            MakeUnconnected(bigStructure);
            AddCycle(bigStructure);
            return CreateUndirectedGraph(bigStructure);
        }

        private static UndirectedGraph GetNotDirectedNotConnectedNotCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            MakeUnconnected(bigStructure);
            return CreateUndirectedGraph(bigStructure);
        }

        private static DirectedGraph GetDirectedConnectedCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            MakeUnconnected(bigStructure);
            AddCycle(bigStructure);
            return CreateDirectedGraph(bigStructure);
        }

        private static DirectedGraph GetDirectedConnectedNotCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            return CreateDirectedGraph(bigStructure);
        }

        private static DirectedGraph GetDirectedNotConnectedCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            MakeUnconnected(bigStructure);
            AddCycle(bigStructure);
            return CreateDirectedGraph(bigStructure);
        }


        private static DirectedGraph GetDirectedNotConnectedNotCyclicGraph()
        {
            IList<DirectedEdge> bigStructure = GetLargeUndirectedConnectedNotCyclicGraphStructure();
            MakeUnconnected(bigStructure);
            return CreateDirectedGraph(bigStructure);
        }
    }
}