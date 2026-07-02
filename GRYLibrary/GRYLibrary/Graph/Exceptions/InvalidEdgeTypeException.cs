using System;

namespace GRYLibrary.Core.Graph.Exceptions
{
    public class InvalidEdgeTypeException : Exception
    {
        public InvalidEdgeTypeException(string message) : base(message)
        {
        }
    }
}