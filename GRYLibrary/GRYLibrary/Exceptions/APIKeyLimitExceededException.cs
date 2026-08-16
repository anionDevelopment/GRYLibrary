using System;

namespace GRYLibrary.Core.Exceptions
{
    public class APIKeyLimitExceededException : Exception
    {
        public APIKeyLimitExceededException() : this("API-Key limit exceeded.")
        {
        }
        public APIKeyLimitExceededException(string message) : base(message)
        {
        }
    }
}