using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Graph.Exceptions;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GRYLibrary.Core.Graph
{
    /// <summary>
    /// Represents a graph
    /// </summary>
    /// <remarks>
    /// This graph does not support two edges between the same two vertices.
    /// </remarks>
    [XmlInclude(typeof(UndirectedGraph))]
    [XmlInclude(typeof(DirectedGraph))]
    public abstract class Graph
    {
        public ISet<Vertex> Vertices => new HashSet<Vertex>(this._Vertices);
        protected ISet<Vertex> _Vertices = new HashSet<Vertex>();
        public abstract ISet<Edge> Edges { get; }

        public abstract void Accept(IGraphVisitor visitor);
        public abstract T Accept<T>(IGraphVisitor<T> visitor);
        public Graph()
        {
        }
        public abstract ISet<Vertex> GetDirectSuccessors(Vertex vertex, bool doNotWalkAgainstDirectedEdges = true);
        public bool SelfLoopIsAllowed
        {
            get => this._SelfLoopIsAllowed;
            set
            {
                if (!value && this.ContainsOneOrMoreSelfLoops())
                {
                    throw new AssertionException("Setting ContainsSelfLoops to false is not possible while the graph contains self-loops.");
                }
                this._SelfLoopIsAllowed = value;
            }
        }
        private bool _SelfLoopIsAllowed = true;
        public Vertex GetVertex(string vertexName)
        {
            foreach (Vertex vertex in this._Vertices)
            {
                if (vertex.Name.Equals(vertexName))
                {
                    return vertex;
                }
            }
            throw new KeyNotFoundException();
        }
        /// <remarks>
        /// This function will also add the vertices which are connected by <paramref name="edge"/> to the list of vertices of this graph.
        /// </remarks>
        public abstract void AddEdge(Edge edge);
        public abstract void RemoveEdge(Edge edge);
        public void AddVertex(Vertex vertex)
        {
            if (this.Vertices.Contains(vertex))
            {
                throw new Exception($"This {nameof(Graph)} does already have a {nameof(Vertex)} with the name '{vertex.Name}'.");
            }
            this._Vertices.Add(vertex);
        }
        public void RemoveAllEdgesWithoutWeight()
        {
            foreach (Edge edge in this.Edges)
            {
                if (edge.Weight == 0)
                {
                    this.RemoveEdge(edge);
                }
            }
        }
        /// <remarks>
        /// This function will also remove the edges which have a connection to this <paramref name="vertex"/> from the list of edges of this graph.
        /// </remarks>
        public void RemoveVertex(Vertex vertex)
        {
            foreach (Edge edge in vertex.GetConnectedEdges())
            {
                this.RemoveEdge(edge);
            }
            this._Vertices.Remove(vertex);
        }
        protected void OnEdgeAdded(Edge edge)
        {
            foreach (Vertex vertex in edge.GetConnectedVertices())
            {
                this._Vertices.Add(vertex);
                vertex.ConnectedEdges.Add(edge);
            }
        }
        protected void OnEdgeRemoved(Edge edge)
        {
            foreach (Vertex vertex in edge.GetConnectedVertices())
            {
                vertex.ConnectedEdges.Remove(edge);
            }
        }
        protected void AddCheck(Edge edge, Vertex connectedVertex1, Vertex connectedVertex2)
        {
            if (!this.SelfLoopIsAllowed && connectedVertex1.Equals(connectedVertex2))
            {
                throw new InvalidGraphStructureException($"Self-loops are not allowed. Change the value of the {nameof(this.SelfLoopIsAllowed)}-property to allow this.");
            }
            if (this.Edges.Where(existingEdge => existingEdge.Name.Equals(edge.Name)).Any())
            {
                throw new InvalidGraphStructureException($"This graph does already have an edge with the name {edge.Name}.");
            }
            if (this.TryGetEdge(connectedVertex1, connectedVertex2, out _))
            {
                throw new InvalidGraphStructureException($"This graph does already have an edge which connects {connectedVertex1.Name} and {connectedVertex2.Name}.");
            }
        }
        public bool ContainsOneOrMoreSelfLoops()
        {
            foreach (Edge edge in this.Edges)
            {
                IEnumerable<Vertex> connectedVertices = edge.GetConnectedVertices();
                if (connectedVertices.Count() != new HashSet<Vertex>(connectedVertices).Count)
                {
                    return true;
                }
            }
            return false;
        }
        public bool ContainsOneOrMoreCycles()
        {
            foreach (Vertex vertex in this.Vertices)
            {
                bool containsCycle = false;
                this.DepthFirstSearch((currentVertex, edges) =>
                {
                    foreach (Vertex successor in currentVertex.GetSuccessorVertices())
                    {
                        if (successor.Equals(vertex))
                        {
                            containsCycle = true;
                            return false;
                        }
                    }
                    return true;
                }, vertex);
                if (containsCycle)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsConnected()
        {
            if (this._Vertices.Count == 0)
            {
                throw new InvalidOperationException("No vertices available.");
            }
            if (this._Vertices.Count == 1)
            {
                return true;
            }
            Vertex startVertex = this._Vertices.First();
            Dictionary<Vertex, bool> visited = [];
            foreach (Vertex vertex in this._Vertices)
            {
                visited.Add(vertex, false);
            }
            this.DepthFirstSearch((v, l) => visited[v] = true, startVertex, false);
            return !visited.ContainsValue(false);
        }

        public double[,] ToAdjacencyMatrix()
        {
            IList<Vertex> vertices = this.GetOrderedVertices();
            int verticesCount = vertices.Count;
            double[,] result = new double[verticesCount, verticesCount];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (this.TryGetEdge(vertices[i], vertices[j], out Edge connection))
                    {
                        result[i, j] = connection.Weight;
                    }
                    else
                    {
                        result[i, j] = 0;
                    }
                }
            }
            return result;
        }

        private IList<Vertex> GetOrderedVertices()
        {
            return [.. this.Vertices.OrderBy(vertex => vertex.Name)];
        }

        /// <remarks>This operations does not work yet due to missing implementation of <see cref="GetAllCyclesThroughASpecificVertex"/>.</remarks>
        public ISet<Cycle> GetAllCycles()
        {
            ISet<Cycle> result = new HashSet<Cycle>();
            foreach (Vertex vertex in this.Vertices)
            {
                result.UnionWith(this.GetAllCyclesThroughASpecificVertex(vertex));
            }
            return result;
        }
        /// <remarks>This operations is not implemented yet.</remarks>
        public ISet<Cycle> GetAllCyclesThroughASpecificVertex(Vertex vertex)
        {
            throw new NotImplementedException();//todo implement using Backtracking
        }

        /// <remarks>This operations is not implemented yet.</remarks>
        public IList<Edge> GetShortestPath(Vertex start, Vertex destination)
        {
            throw new NotImplementedException();//todo implement using Dijkstra's algorithm with Fibonacci heap
        }

        public override string ToString()
        {
            double[,] matrix = this.ToAdjacencyMatrix();
            string[] table = TableGenerator.Generate(matrix, new TableGenerator.ASCIITable(), value => value.ToString(), this.GetOrderedVertices().ToArray());
            return string.Join(Environment.NewLine, table);
        }
        public abstract ISet<Edge> GetDirectSuccessorEdges(Vertex vertex);
        public void BreadthFirstSearch(Func<Vertex/*current vertex*/, IList<Edge>/*path*/, bool/*continue search*/> customAction)
        {
            this.BreadthFirstSearch(customAction, this.Vertices.First());
        }

        public void BreadthFirstSearch(Func<Vertex/*current vertex*/, IList<Edge>/*path*/, bool/*continue search*/> customAction, Vertex startVertex)
        {
            this.InitializeSearchAndDoSomeChecks(startVertex, out Dictionary<Vertex, bool> visitedMap);
            Queue<Tuple<Vertex, IList<Edge>>> queue = new();
            visitedMap[startVertex] = true;
            List<Edge> initialList = [];
            if (!customAction(startVertex, initialList))
            {
                return;
            }
            queue.Enqueue(new Tuple<Vertex, IList<Edge>>(startVertex, initialList));
            while (queue.Count != 0)
            {
                Tuple<Vertex, IList<Edge>> currentVertex = queue.Dequeue();
                foreach (Vertex successor in this.GetDirectSuccessors(currentVertex.Item1))
                {
                    if (!visitedMap[successor])
                    {
                        visitedMap[successor] = true;
                        List<Edge> successorPath = [.. currentVertex.Item2];
                        if (this.TryGetEdge(currentVertex.Item1, successor, out Edge edge))
                        {
                            successorPath.Add(edge);
                        }
                        else
                        {
                            successorPath.Add(this.GetNewEdgeBetween(currentVertex.Item1, successor));
                        }
                        if (!customAction(successor, successorPath))
                        {
                            return;
                        }
                        queue.Enqueue(new Tuple<Vertex, IList<Edge>>(successor, successorPath));
                    }
                }
            }
        }

        public void DepthFirstSearch(Func<Vertex/*current vertex*/, IList<Edge>/*path*/, bool/*continue search*/> customAction, bool doNotWalkAgainstDirectedEdges = true)
        {
            this.DepthFirstSearch(customAction, this.Vertices.First(), doNotWalkAgainstDirectedEdges);
        }

        public void DepthFirstSearch(Func<Vertex/*current vertex*/, IList<Edge>/*path*/, bool/*continue search*/> customAction, Vertex startVertex, bool doNotWalkAgainstDirectedEdges = true)
        {
            this.InitializeSearchAndDoSomeChecks(startVertex, out Dictionary<Vertex, bool> visitedMap);
            Stack<Tuple<Vertex, IList<Edge>>> stack = new();
            stack.Push(new Tuple<Vertex, IList<Edge>>(startVertex, []));
            while (stack.Count > 0)
            {
                Tuple<Vertex, IList<Edge>> currentVertex = stack.Pop();
                if (!visitedMap[currentVertex.Item1])
                {
                    visitedMap[currentVertex.Item1] = true;
                    if (!customAction(currentVertex.Item1, currentVertex.Item2))
                    {
                        return;
                    }
                    foreach (Vertex successor in this.GetDirectSuccessors(currentVertex.Item1, doNotWalkAgainstDirectedEdges))
                    {
                        List<Edge> successorPath = [.. currentVertex.Item2];
                        if (this.TryGetEdge(currentVertex.Item1, successor, out Edge edge))
                        {
                            successorPath.Add(edge);
                        }
                        else
                        {
                            successorPath.Add(this.GetNewEdgeBetween(currentVertex.Item1, successor));
                        }
                        stack.Push(new Tuple<Vertex, IList<Edge>>(successor, successorPath));
                    }
                }
            }
        }

        internal abstract Edge GetNewEdgeBetween(Vertex item1, Vertex successor);

        private void InitializeSearchAndDoSomeChecks(Vertex startVertex, out Dictionary<Vertex, bool> visitedMap)
        {
            if (!this.Vertices.Contains(startVertex))
            {
                throw new Exceptions.InternalAlgorithmException($"Vertex '{startVertex}' is not contained in this graph.");
            }
            visitedMap = [];
            foreach (Vertex vertex in this.Vertices)
            {
                visitedMap.Add(vertex, false);
            }
        }

        public bool TryGetEdge(Vertex source, Vertex target, out Edge result)
        {
            foreach (Edge edge in this.Edges)
            {
                if (edge.Connects(source, target))
                {
                    result = edge;
                    return true;
                }
            }
            result = null;
            return false;
        }

        /// <returns>
        /// Returns true if and only if the graphs have the same structure.
        /// </returns>
        /// <remarks>
        /// This function ignores properties like <see cref="SelfLoopIsAllowed"/> or the name of the edges and vertices.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            Graph typedObj = obj as Graph;
            double[,] adjacencyMatrix1 = this.ToAdjacencyMatrix();
            double[,] adjacencyMatrix2 = typedObj.ToAdjacencyMatrix();
            bool arraysEquals = Utilities.TwoDimensionalArrayEquals(adjacencyMatrix1, adjacencyMatrix2);
            bool verticesEquals = this.Vertices.SequenceEqual(typedObj.Vertices);
            return arraysEquals && verticesEquals;
        }
        public override int GetHashCode()
        {
            return this._Vertices.Count.GetHashCode() ^ this.Edges.Count.GetHashCode();
        }

        /// <remarks>This operations is not implemented yet.</remarks>
        public bool IsSubgraph(Graph subgraph, out IDictionary<Vertex, Vertex> mappingFromSubgraphToThisGraph)
        {
            throw new NotImplementedException();
        }

        /// <remarks>This operations does not work yet due to missing implementation of <see cref="IsSubgraph"/>.</remarks>
        public bool IsSubgraphOf(Graph graph, out IDictionary<Vertex, Vertex> mappingFromgraphToThisGraph)
        {
            bool result = graph.IsSubgraph(this, out IDictionary<Vertex, Vertex> mappingFromSubgraphToThisGraph);
            if (result)
            {
                mappingFromgraphToThisGraph = mappingFromSubgraphToThisGraph.ToDictionary(keyValuePair => keyValuePair.Value, keyValuePair => keyValuePair.Key);
            }
            else
            {
                mappingFromgraphToThisGraph = null;
            }
            return result;
        }
        /// <remarks>This operations does not work yet due to missing implementation of <see cref="IsSubgraph"/>.</remarks>
        public bool IsIsomorphic(Graph otherGraph, out IDictionary<Vertex, Vertex> bijectionFromOtherGraphToThisGraph)
        {
            if (!this.GetType().Equals(otherGraph.GetType()))
            {
                throw new UnallowedOperationException($"Graphs of types {this.GetType().FullName} and {otherGraph.GetType().FullName} are not comparable.");
            }
            if (this._Vertices.Count != otherGraph._Vertices.Count || this.Edges.Count != otherGraph.Edges.Count)
            {
                bijectionFromOtherGraphToThisGraph = null;
                return false;
            }
            if (this.IsSubgraphOf(otherGraph, out IDictionary<Vertex, Vertex> mappingFromgraphToThisGraph))
            {
                bijectionFromOtherGraphToThisGraph = mappingFromgraphToThisGraph;
                return true;
            }
            else
            {
                bijectionFromOtherGraphToThisGraph = null;
                return false;
            }
        }
        /// <remarks>This operations does not work yet due to missing implementation of <see cref="GetAllCyclesThroughASpecificVertex"/>.</remarks>
        public bool HasHamiltonianCycle(out Cycle result)
        {
            foreach (Cycle cycle in this.GetAllCycles())
            {
                if (cycle.Edges.Count == this._Vertices.Count)
                {
                    result = cycle;
                    return true;
                }
            }
            result = null;
            return false;
        }
        public int GetMinimumDegree()
        {
            return this.Vertices.Min(vertex => vertex.Degree());
        }

        public int GetMaximumDegree()
        {
            return this.Vertices.Max(vertex => vertex.Degree());
        }
    }
    public interface IGraphVisitor
    {
        void Handle(UndirectedGraph graph);
        void Handle(DirectedGraph graph);
    }
    public interface IGraphVisitor<T>
    {
        T Handle(UndirectedGraph graph);
        T Handle(DirectedGraph graph);
    }
}