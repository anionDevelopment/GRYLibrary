using System;

namespace GRYLibrary.Core.Exceptions
{
    public class InitializationException : Exception
    {
        public InitializationException(string message) : base(message)
        {
        }
    }
}
