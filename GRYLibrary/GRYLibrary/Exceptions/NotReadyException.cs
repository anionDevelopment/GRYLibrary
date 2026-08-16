using System;

namespace GRYLibrary.Core.Exceptions
{
    public class NotReadyException : Exception
    {
        public NotReadyException() : base()
        {
        }
        public NotReadyException(string message) : base(message)
        {
        }
        public NotReadyException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
