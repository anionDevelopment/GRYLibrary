
namespace GRYLibrary.Core.APIServer.MidT.RLog
{
    public interface ISupportRequestLoggingMiddleware : ISupportedMiddleware
    {
        public IRequestLoggingConfiguration ConfigurationForLoggingMiddleware { get; }
    }
}
