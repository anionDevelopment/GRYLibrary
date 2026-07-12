using System;

namespace GRYLibrary.Core.Exceptions
{
    public class DependencyNotAvailableException : Exception
    {
        public DependencyNotAvailableException() : this("Dependency not available.")
        {
        }
        public DependencyNotAvailableException(string message) : base(message)
        {
        }
        public DependencyNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
