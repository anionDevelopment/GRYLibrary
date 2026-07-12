namespace GRYLibrary.Core.APIServer.MidT.RateLimit
{
    /// <summary>
    /// Configuration for a <see cref="RateLimitingMiddleware"/>. Both limits are per fixed
    /// one-minute window. A value of 0 (or negative) disables the respective limit.
    /// </summary>
    public interface IRateLimitingConfiguration
    {
        /// <summary>
        /// Maximum number of requests per minute per client IP for anonymous requests (requests
        /// that do not carry an authentication cookie).
        /// </summary>
        int AnonymousRequestsPerMinutePerIP { get; set; }

        /// <summary>
        /// Maximum number of requests per minute per authenticated client for requests that carry
        /// an authentication cookie (i.e. an authentication attempt or an authenticated session).
        /// </summary>
        int AuthenticatedRequestsPerMinutePerUser { get; set; }
    }
}
