namespace GRYLibrary.Core.APIServer.MidT.Exception
{
    public interface ISupportExceptionManagerMiddleware : ISupportedMiddleware
    {
        public IExceptionManagerConfiguration ConfigurationForExceptionManagerMiddleware { get; }
    }
}
