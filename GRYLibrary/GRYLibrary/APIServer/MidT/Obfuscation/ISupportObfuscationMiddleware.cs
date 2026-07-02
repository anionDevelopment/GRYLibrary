namespace GRYLibrary.Core.APIServer.MidT.Obfuscation
{
    public interface ISupportObfuscationMiddleware : ISupportedMiddleware
    {
        public IObfuscationConfiguration ConfigurationForObfuscationMiddleware { get; }
    }
}
