namespace GRYLibrary.Core.APIServer.MidT.Auth
{
    public interface ISupportAuthenticationMiddleware : ISupportedMiddleware
    {
        public IAuthenticationConfiguration ConfigurationForAuthenticationMiddleware { get; }
    }
}
