using System;

namespace GRYLibrary.Core.Graph.Exceptions
{
    public class InvalidGraphStructureException : Exception
    {
        public InvalidGraphStructureException(string message) : base(message)
        {
        }
    }
}