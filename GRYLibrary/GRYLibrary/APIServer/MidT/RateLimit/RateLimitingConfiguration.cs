namespace GRYLibrary.Core.APIServer.MidT.RateLimit
{
    /// <summary>
    /// Default <see cref="IRateLimitingConfiguration"/>. The defaults (1000 anonymous requests per
    /// minute per IP, 30 authenticated requests per minute per user) are sensible starting points;
    /// consumers persist this in their application configuration so operators can tune it.
    /// </summary>
    public class RateLimitingConfiguration : IRateLimitingConfiguration
    {
        public int AnonymousRequestsPerMinutePerIP { get; set; } = 1000;
        public int AuthenticatedRequestsPerMinutePerUser { get; set; } = 30;
    }
}
