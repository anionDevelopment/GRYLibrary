using System;

namespace GRYLibrary.Core.Exceptions
{
    public class ProcessStartException : Exception
    {
        public ProcessStartException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}