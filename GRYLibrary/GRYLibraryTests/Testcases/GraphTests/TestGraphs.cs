using GRYLibrary.Core.Graph;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    internal class TestGraphs
    {
        /// <returns>
        /// Returns a graph with the following structure:
        /// v0->v1->v2->v3->v4->v5
        /// ^                   |
        /// └-------------------┘
        /// </returns>
        internal static DirectedGraph GetTestConnectedGraphWithSimpleLoop()
        {
            DirectedGraph graph = new();
            Vertex v0 = new("v0");
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            Vertex v3 = new("v3");
            Vertex v4 = new("v4");
            Vertex v5 = new("v5");
            graph.AddEdge(new DirectedEdge(v0, v1, "e1"));
            graph.AddEdge(new DirectedEdge(v1, v2, "e2"));
            graph.AddEdge(new DirectedEdge(v2, v3, "e3"));
            graph.AddEdge(new DirectedEdge(v3, v4, "e4"));
            graph.AddEdge(new DirectedEdge(v4, v5, "e5"));
            graph.AddEdge(new DirectedEdge(v5, v0, "e6"));
            return graph;
        }
        /// <returns>
        /// Returns a graph with the following structure:
        /// v0->v1->v2->v3->v4->v5
        /// </returns>
        internal static DirectedGraph GetTestGraphWithoutLoop2()
        {
            DirectedGraph graph = new();
            Vertex v0 = new("v0");
            Vertex v1 = new("v1");
            Vertex v2 = new("v2");
            Vertex v3 = new("v3");
            Vertex v4 = new("v4");
            Vertex v5 = new("v5");
            graph.AddEdge(new DirectedEdge(v0, v1, "e1"));
            graph.AddEdge(new DirectedEdge(v1, v2, "e2"));
            graph.AddEdge(new DirectedEdge(v2, v3, "e3"));
            graph.AddEdge(new DirectedEdge(v3, v4, "e4"));
            graph.AddEdge(new DirectedEdge(v4, v5, "e5"));
            return graph;
        }

        internal static UndirectedGraph GetTestGraphWithoutLoop()
        {
            UndirectedGraph graph = new();
            Vertex v1 = new(nameof(v1));
            graph.AddVertex(v1);
            Vertex v2 = new(nameof(v2));
            graph.AddVertex(v2);
            Vertex v3 = new(nameof(v3));
            graph.AddVertex(v3);
            Vertex v4 = new(nameof(v4));
            graph.AddVertex(v4);
            Vertex v5 = new(nameof(v5));
            graph.AddVertex(v5);
            Vertex v6 = new(nameof(v6));
            graph.AddVertex(v6);
            Vertex v7 = new(nameof(v7));
            graph.AddVertex(v7);
            Vertex v8 = new(nameof(v8));
            graph.AddVertex(v8);
            Vertex v9 = new(nameof(v9));
            graph.AddVertex(v9);
            Vertex v10 = new(nameof(v10));
            graph.AddVertex(v10);

            UndirectedEdge e1 = new(new Vertex[] { v1, v2 }, nameof(e1));
            graph.AddEdge(e1);
            UndirectedEdge e2 = new(new Vertex[] { v1, v3 }, nameof(e2));
            graph.AddEdge(e2);
            UndirectedEdge e3 = new(new Vertex[] { v1, v4 }, nameof(e3));
            graph.AddEdge(e3);
            UndirectedEdge e4 = new(new Vertex[] { v2, v5 }, nameof(e4));
            graph.AddEdge(e4);
            UndirectedEdge e5 = new(new Vertex[] { v3, v6 }, nameof(e5));
            graph.AddEdge(e5);
            UndirectedEdge e6 = new(new Vertex[] { v3, v7 }, nameof(e6));
            graph.AddEdge(e6);
            UndirectedEdge e7 = new(new Vertex[] { v4, v8 }, nameof(e7));
            graph.AddEdge(e7);
            UndirectedEdge e8 = new(new Vertex[] { v5, v9 }, nameof(e8));
            graph.AddEdge(e8);
            UndirectedEdge e9 = new(new Vertex[] { v6, v10 }, nameof(e9));
            graph.AddEdge(e9);

            return graph;

        }
    }
}