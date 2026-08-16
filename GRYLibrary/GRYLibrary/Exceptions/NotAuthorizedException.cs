using System;
namespace GRYLibrary.Core.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : this("Not authorized")
        {
        }
        public NotAuthorizedException(string message) : base(message)
        {
        }
    }
}