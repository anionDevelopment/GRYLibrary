namespace GRYLibrary.Core.APIServer.MidT.Aut
{
    public interface ISupportAuthorizationMiddleware : ISupportedMiddleware
    {
        public IAuthorizationConfiguration ConfigurationForAuthorizationMiddleware { get; }
    }
}
