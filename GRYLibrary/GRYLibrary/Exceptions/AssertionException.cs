using System;

namespace GRYLibrary.Core.Exceptions
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}