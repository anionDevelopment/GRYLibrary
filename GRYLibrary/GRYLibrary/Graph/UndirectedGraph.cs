using GRYLibrary.Core.Graph.Exceptions;
using System.Collections.Generic;

namespace GRYLibrary.Core.Graph
{
    public sealed class UndirectedGraph : Graph
    {
        public override ISet<Edge> Edges => new HashSet<Edge>(this._UndirectedEdges);
        public ISet<Edge> UndirectedEdges => new HashSet<Edge>(this._UndirectedEdges);
        private readonly ISet<UndirectedEdge> _UndirectedEdges = new HashSet<UndirectedEdge>();
        public UndirectedGraph() { }
        public override void Accept(IGraphVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IGraphVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }

        /// <returns>
        /// Returns a set of all vertices which have a connection to this vertex in this graph.
        /// </returns>
        /// <remarks>
        /// If this graph contains a selfloop with this vertex then the result-set will also contain this vertex.
        /// </remarks>
        public override ISet<Vertex> GetDirectSuccessors(Vertex vertex, bool doNotWalkAgainstDirectedEdges = true)
        {
            HashSet<Vertex> result = [];
            foreach (UndirectedEdge edge in vertex.GetConnectedEdges())
            {
                List<Vertex> vertices = [.. edge.ConnectedVertices];
                if (vertices[0].Equals(vertices[1]) && vertices[0].Equals(vertex))
                {
                    result.Add(vertex);
                }
                else
                {
                    if (vertices[0].Equals(vertex))
                    {
                        result.Add(vertices[1]);
                    }
                    if (vertices[1].Equals(vertex))
                    {
                        result.Add(vertices[0]);
                    }
                }
            }
            return result;
        }
        public override ISet<Edge> GetDirectSuccessorEdges(Vertex vertex)
        {
            HashSet<Edge> result = [];
            foreach (Edge edge in vertex.GetConnectedEdges())
            {
                UndirectedEdge undirectedEdge = (UndirectedEdge)edge;
                IList<Vertex> connectedVertices = undirectedEdge.ConnectedVertices;
                bool sourceIsThis = connectedVertices[0].Equals(vertex);
                bool targetIsThis = connectedVertices[1].Equals(vertex);
                if (sourceIsThis && targetIsThis)
                {
                    result.Add(edge);
                }
                else
                {
                    if (sourceIsThis)
                    {
                        result.Add(edge);
                    }
                    if (targetIsThis)
                    {
                        result.Add(edge);
                    }
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public override void AddEdge(Edge edge)
        {
            if (edge is not UndirectedEdge)
            {
                throw new InvalidEdgeTypeException($"{nameof(UndirectedGraph)}-objects can only have edges of type {nameof(UndirectedEdge)}");
            }
            UndirectedEdge undirectedEdge = (UndirectedEdge)edge;
            List<Vertex> connectedVertices = [.. undirectedEdge.ConnectedVertices];
            this.AddCheck(edge, connectedVertices[0], connectedVertices[1]);
            this._UndirectedEdges.Add((UndirectedEdge)edge);
            this.OnEdgeAdded(edge);
        }

        /// <inheritdoc/>
        public override void RemoveEdge(Edge edge)
        {
            if (edge is UndirectedEdge)
            {
                this._UndirectedEdges.Remove((UndirectedEdge)edge);
                this.OnEdgeRemoved(edge);
            }
        }
        public override string ToString()
        {
            return base.ToString();
        }

        internal override Edge GetNewEdgeBetween(Vertex vertex1, Vertex vertex2)
        {
            return new UndirectedEdge(new Vertex[] { vertex1, vertex2 });
        }
    }
}