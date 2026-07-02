using System;

namespace GRYLibrary.Core.Exceptions
{
    public class InternalAlgorithmException : Exception
    {

        public InternalAlgorithmException(string errorId) : base(CalculateMessage(errorId))
        {
        }

        public InternalAlgorithmException(string errorId, string message) : base(CalculateMessage(errorId, message))
        {
        }

        public InternalAlgorithmException(Exception innerException, string errorId) : base(CalculateMessage(errorId), innerException)
        {
        }

        public InternalAlgorithmException(Exception innerException, string errorId, string message) : base(CalculateMessage(errorId, message), innerException)
        {
        }

        private static string CalculateMessage(string message)
        {
            string result = $"Internal algorithm error.";
            if (message != null)
            {
                result = $"{result}; Message: {message}";
            }
            return result;
        }

        private static string CalculateMessage(string errorId, string message)
        {
            string result = $"Internal algorithm error. Error-information: {errorId}";
            if (message != null)
            {
                result = $"{result}; Message: {message}";
            }
            return result;
        }
    }
}