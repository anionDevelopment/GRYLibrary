using System;

namespace GRYLibrary.Core.Exceptions
{
    /// <summary>
    /// Represents an exception with a user-friendly error-message which can be displayed also to a non-technical-user.
    /// </summary>
    public class UserFormattedException : Exception
    {
        //TODO add option to translate messages
        public UserFormattedException(string message) : base(message)
        {
        }
    }
}