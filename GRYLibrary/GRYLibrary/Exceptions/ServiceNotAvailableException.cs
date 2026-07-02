using System;

namespace GRYLibrary.Core.Exceptions
{
    public class ServiceNotAvailableException : Exception
    {
        public ServiceNotAvailableException() : base("Service currently not available.")
        {
        }
        public ServiceNotAvailableException(string message) : base(message)
        {
        }
        public ServiceNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}