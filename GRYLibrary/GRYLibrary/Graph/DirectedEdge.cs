using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Graph
{
    public class DirectedEdge : Edge
    {
        public Vertex Source { get; private set; }
        public Vertex Target { get; private set; }
        public DirectedEdge() { }
        public DirectedEdge(Vertex source, Vertex target) : this(source, target, CalculateName(source, target))
        {
        }

        private static string CalculateName(Vertex source, Vertex target)
        {
            return $"({source}->{target})";
        }

        public DirectedEdge(Vertex source, Vertex target, string name, double weight = 1) : base(name, weight)
        {
            this.Source = source;
            this.Target = target;
        }

        internal UndirectedEdge ToUndirectedEdge(Vertex equalSourceInUndirectedGraph, Vertex equalTargetInUndirectedGraph)
        {
            return new UndirectedEdge(new Vertex[] { equalSourceInUndirectedGraph, equalTargetInUndirectedGraph }, this.Name, this.Weight);
        }

        public override bool Connects(Vertex fromVertex, Vertex toVertex)
        {
            return this.Source.Equals(fromVertex) && this.Target.Equals(toVertex);
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }
            DirectedEdge typedObject = (DirectedEdge)obj;
            if (!this.Source.Equals(typedObject.Source))
            {
                return false;
            }
            if (!this.Target.Equals(typedObject.Target))
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode());
        }

        public override IEnumerable<Vertex> GetConnectedVertices()
        {
            return new Vertex[] { this.Source, this.Target };
        }

        public override void Accept(IEdgeVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IEdgeVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }

        public override IEnumerable<Vertex> GetInputs()
        {
            return new Vertex[] { this.Source };
        }

        public override IEnumerable<Vertex> GetOutputs()
        {
            return new Vertex[] { this.Target };
        }

        public override IEnumerable<Vertex> GetOtherConnectedVerticesVisitor(Vertex vertex)
        {
            if (this.Source.Equals(vertex))
            {
                return new Vertex[] { this.Target };
            }
            if (this.Target.Equals(vertex))
            {
                return new Vertex[] { this.Source };
            }
            throw new Exception();
        }
    }
}