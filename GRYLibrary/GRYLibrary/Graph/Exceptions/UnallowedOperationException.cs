using System;

namespace GRYLibrary.Core.Graph.Exceptions
{
    public class UnallowedOperationException : Exception
    {
        public UnallowedOperationException(string message) : base(message)
        {
        }
    }
}