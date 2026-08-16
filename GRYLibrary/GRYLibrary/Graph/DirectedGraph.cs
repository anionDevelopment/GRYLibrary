using GRYLibrary.Core.Graph.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.Graph
{
    public sealed class DirectedGraph : Graph
    {
        public override ISet<Edge> Edges => new HashSet<Edge>(this._DirectedEdges);
        public ISet<Edge> DirectedEdges => new HashSet<Edge>(this._DirectedEdges);
        private readonly ISet<DirectedEdge> _DirectedEdges = new HashSet<DirectedEdge>();
        public DirectedGraph() { }

        public override void Accept(IGraphVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IGraphVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }

        public UndirectedGraph ToUndirectedGraph()
        {
            UndirectedGraph result = new();
            foreach (Vertex vertex in this.Vertices)
            {
                result.AddVertex(new Vertex(vertex.Name));
            }
            foreach (DirectedEdge edge in this._DirectedEdges)
            {
                result.AddEdge(edge.ToUndirectedEdge(result.GetVertex(edge.Source.Name), result.GetVertex(edge.Target.Name)));
            }
            return result;
        }
        /// <returns>
        /// Returns a set of all vertices which are a direct successor of this vertex in this graph.
        /// </returns>
        /// <remarks>If this graph contains a selfloop with this vertex then the result-set will also contain this vertex.
        /// The runtime of this function is &lt;=O(|this.Edges|).
        /// </remarks>
        public override ISet<Vertex> GetDirectSuccessors(Vertex vertex, bool doNotWalkAgainstDirectedEdges = true)
        {
            HashSet<Vertex> result = [];
            foreach (DirectedEdge edge in vertex.GetConnectedEdges())
            {
                if (edge.Source.Equals(vertex))
                {
                    result.Add(edge.Target);
                }
                if (edge.Target.Equals(vertex) && !doNotWalkAgainstDirectedEdges)
                {
                    result.Add(edge.Source);
                }
            }
            return result;
        }

        /// <returns>Returns a set of all vertices which are a direct predecessor of this vertex in this graph.</returns>
        /// <remarks>
        /// If this graph contains a selfloop with this vertex then the result-set will also contain this vertex.
        /// The runtime of this function is &lt;=O(|this.Edges|).
        /// </remarks>
        public ISet<Vertex> GetDirectPredecessors(Vertex vertex)
        {
            HashSet<Vertex> result = [];
            foreach (DirectedEdge edge in vertex.GetConnectedEdges())
            {
                if (edge.Target.Equals(vertex))
                {
                    result.Add(edge.Source);
                }
            }
            return result;
        }
        public bool HasIncomingEdge(Vertex vertex)
        {
            foreach (DirectedEdge edge in vertex.GetConnectedEdges())
            {
                if (edge.Target.Equals(vertex))
                {
                    return true;
                }
            }
            return false;
        }
        public override ISet<Edge> GetDirectSuccessorEdges(Vertex vertex)
        {
            return new HashSet<Edge>(this.GetOutgoingEdges(vertex).OfType<Edge>());
        }

        public ISet<DirectedEdge> GetIncomingEdges(Vertex vertex)
        {
            HashSet<DirectedEdge> result = [];
            foreach (DirectedEdge edge in vertex.GetConnectedEdges())
            {
                if (edge.Target.Equals(vertex))
                {
                    result.Add(edge);
                }
            }
            return result;
        }
        public ISet<DirectedEdge> GetOutgoingEdges(Vertex vertex)
        {
            HashSet<DirectedEdge> result = [];
            foreach (DirectedEdge edge in vertex.GetConnectedEdges())
            {
                if (edge.Source.Equals(vertex))
                {
                    result.Add(edge);
                }
            }
            return result;
        }

        /// <returns>
        /// Returns a <see cref="DirectedGraph"/> which has one vertex per row of <paramref name="adjacencyMatrix"/> and one edge for every entry of
        /// <paramref name="adjacencyMatrix"/> which is not 0 (a value of 0 means that there is no connection between the two vertices).
        /// </returns>
        public static DirectedGraph CreateByAdjacencyMatrix(double[,] adjacencyMatrix)
        {
            DirectedGraph graph = new();
            Tuple<IList<DirectedEdge>, IList<Vertex>> items = ParseAdjacencyMatrix(adjacencyMatrix);
            foreach (Vertex item in items.Item2)
            {
                graph.AddVertex(item);
            }
            foreach (DirectedEdge item in items.Item1)
            {
                // AddEdge (and not _DirectedEdges.Add) is used so that the edges are also registered at their vertices.
                // Otherwise operations like GetDirectSuccessors would not find any connection in the resulting graph.
                graph.AddEdge(item);
            }
            return graph;
        }
        private static Tuple<IList<DirectedEdge>, IList<Vertex>> ParseAdjacencyMatrix(double[,] adjacencyMatrix)
        {
            if (adjacencyMatrix.GetLength(0) != adjacencyMatrix.GetLength(1))
            {
                throw new Exception("The row-count must be equal to the column-count.");
            }
            IList<Vertex> vertices = [];
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                Vertex newVertex = new("Vertex_" + (i + 1).ToString());
                vertices.Add(newVertex);
            }
            IList<DirectedEdge> edges = [];
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
                {
                    if (adjacencyMatrix[i, j] == 0)
                    {
                        continue;//a weight of 0 means that there is no edge between these two vertices
                    }
                    DirectedEdge newEdge = new(vertices[i], vertices[j], nameof(Edge) + "_" + (i + 1).ToString() + "_" + (j + 1).ToString())
                    {
                        Weight = adjacencyMatrix[i, j]
                    };
                    edges.Add(newEdge);
                }
            }
            return new Tuple<IList<DirectedEdge>, IList<Vertex>>(edges, vertices);
        }

        /// <inheritdoc/>
        public override void AddEdge(Edge edge)
        {
            if (edge is not DirectedEdge)
            {
                throw new InvalidEdgeTypeException($"{nameof(DirectedGraph)}-objects can only have edges of type {nameof(DirectedEdge)}");
            }
            DirectedEdge directedEdge = (DirectedEdge)edge;
            this.AddCheck(edge, ((DirectedEdge)edge).Source, ((DirectedEdge)edge).Target);
            this._DirectedEdges.Add((DirectedEdge)edge);
            this.OnEdgeAdded(edge);
        }
        /// <inheritdoc/>
        public override void RemoveEdge(Edge edge)
        {
            if (edge is DirectedEdge)
            {
                this._DirectedEdges.Remove((DirectedEdge)edge);
                this.OnEdgeRemoved(edge);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        internal override Edge GetNewEdgeBetween(Vertex source, Vertex target)
        {
            return new DirectedEdge(source, target);
        }
    }
}