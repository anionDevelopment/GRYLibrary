using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GRYLibrary.Core.Graph
{
    [XmlInclude(typeof(UndirectedEdge))]
    [XmlInclude(typeof(DirectedEdge))]
    public abstract class Edge
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public Edge() : this(CalculateEdgeName())
        {
        }
        public Edge(string name, double weight = 1)
        {
            this.Name = name;
            this.Weight = weight;
        }
        public abstract bool Connects(Vertex fromVertex, Vertex toVertex);

        public abstract IEnumerable<Vertex> GetConnectedVertices();

        private static string CalculateEdgeName()
        {
            return $"{nameof(Edge)}_{Guid.NewGuid().ToString()[..8]}";
        }

        public override bool Equals(object obj)
        {
            Edge typedObject = obj as Edge;
            if (typedObject is null)
            {
                return false;
            }
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            if (!this.Name.Equals(typedObject.Name))
            {
                return false;
            }
            if (!this.Weight.NullSafeEquals(typedObject.Weight))
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public abstract void Accept(IEdgeVisitor visitor);
        public abstract T Accept<T>(IEdgeVisitor<T> visitor);

        public abstract IEnumerable<Vertex> GetInputs();
        public abstract IEnumerable<Vertex> GetOutputs();
        public abstract IEnumerable<Vertex> GetOtherConnectedVerticesVisitor(Vertex vertex);
    }
    public interface IEdgeVisitor
    {
        void Handle(UndirectedEdge edge);
        void Handle(DirectedEdge edge);
    }
    public interface IEdgeVisitor<T>
    {
        T Handle(UndirectedEdge edge);
        T Handle(DirectedEdge edge);
    }
}