using System;

namespace GRYLibrary.Core.Exceptions
{
    /// <summary>
    /// Represents an exception which indicates that the algorithm where this exception will be thrown is supposed to be aborted.
    /// </summary>
    public class AbortException : Exception
    {
        public AbortException(Exception innerException) : this(innerException, "Action was aborted.")
        {
        }
        public AbortException(Exception innerException, string message) : base(message, innerException)
        {
        }
    }
}
