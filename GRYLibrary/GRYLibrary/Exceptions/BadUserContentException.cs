namespace GRYLibrary.Core.Exceptions
{
    public class BadUserContentException : UserFormattedException
    {
        public BadUserContentException(string message) : base(message)
        {
        }
    }
}
