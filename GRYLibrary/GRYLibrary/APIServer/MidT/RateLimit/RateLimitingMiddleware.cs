using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Verbs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.RateLimit
{
    /// <summary>
    /// Fixed-window request rate-limiter. Requests are grouped into a bucket per client and
    /// counted per calendar minute; once a bucket exceeds its configured limit within the current
    /// minute, further requests receive HTTP 429 until the next minute starts.
    ///
    /// Two client classes are distinguished (see <see cref="ClassifyRequest"/>):
    /// <list type="bullet">
    /// <item>requests carrying an authentication cookie are limited per authenticated identity
    /// (<see cref="IRateLimitingConfiguration.AuthenticatedRequestsPerMinutePerUser"/>);</item>
    /// <item>all other requests are limited per client IP
    /// (<see cref="IRateLimitingConfiguration.AnonymousRequestsPerMinutePerIP"/>).</item>
    /// </list>
    /// Because deciding "is this an authentication attempt?" and deriving the identity is
    /// application-specific (the cookie shape differs per app), those two steps are abstract.
    /// The middleware is intended to run early in the pipeline (before authentication).
    /// </summary>
    public abstract class RateLimitingMiddleware : AbstractMiddleware
    {
        private readonly IRateLimitingConfiguration _Configuration;
        private readonly ITimeService _TimeService;
        private readonly IAPIServerCommandlineParameter _CommandlineParameter;
        private readonly object _Lock = new object();
        private readonly Dictionary<string, FixedWindowCounter> _Counters = new Dictionary<string, FixedWindowCounter>();
        // Bound the bucket dictionary: prune stale windows once it grows past this size.
        private const int PruneThreshold = 10000;

        protected RateLimitingMiddleware(RequestDelegate next, IRateLimitingConfiguration configuration, ITimeService timeService, IAPIServerCommandlineParameter commandlineParameter) : base(next)
        {
            this._Configuration = configuration;
            this._TimeService = timeService;
            this._CommandlineParameter = commandlineParameter;
        }

        public override Task Invoke(HttpContext context)
        {
            // The limit only applies to a real run. A test- or analysis-run drives the application without the
            // pauses a human makes between two requests, so it would reach the limit within seconds and would
            // then be testing the rate-limiting instead of what it wants to check. The limits themselves are
            // chosen for the productive operation and must not be weakened for that reason.
            if (!this._CommandlineParameter.RealRun)
            {
                return this._Next(context);
            }
            (string key, int limit) = this.ClassifyRequest(context);
            // limit <= 0 means "disabled" for that client-class.
            if (limit > 0 && this.RegisterRequestAndCheckExceeded(key, limit))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = this.GetTooManyRequestsContentType();
                return context.Response.WriteAsync(this.GetTooManyRequestsBody(context));
            }
            return this._Next(context);
        }

        /// <summary>
        /// Returns the bucket key and the applicable per-minute limit for the given request.
        /// Authenticated requests are keyed per identity; everything else per IP.
        /// </summary>
        protected virtual (string key, int limit) ClassifyRequest(HttpContext context)
        {
            if (this.RequestCarriesAuthentication(context))
            {
                return ($"user:{this.GetAuthenticatedClientIdentity(context)}", this._Configuration.AuthenticatedRequestsPerMinutePerUser);
            }
            return ($"ip:{this.GetAnonymousClientIdentity(context)}", this._Configuration.AnonymousRequestsPerMinutePerIP);
        }

        /// <summary>Whether the request carries an authentication cookie (an auth attempt/session).</summary>
        protected abstract bool RequestCarriesAuthentication(HttpContext context);

        /// <summary>The per-user key for an authenticated request (e.g. the username in the cookie).</summary>
        protected abstract string GetAuthenticatedClientIdentity(HttpContext context);

        /// <summary>The per-client key for an anonymous request. Defaults to the remote IP.</summary>
        protected virtual string GetAnonymousClientIdentity(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        protected virtual string GetTooManyRequestsContentType()
        {
            return "text/plain";
        }

        protected virtual string GetTooManyRequestsBody(HttpContext context)
        {
            return "Too many requests. Please slow down and try again in a minute.";
        }

        private bool RegisterRequestAndCheckExceeded(string key, int limit)
        {
            long currentMinute = this._TimeService.GetCurrentTimeInUTCAsDateTimeOffset().ToUnixTimeSeconds() / 60;
            lock (this._Lock)
            {
                if (!this._Counters.TryGetValue(key, out FixedWindowCounter counter) || counter.WindowMinute != currentMinute)
                {
                    counter = new FixedWindowCounter(currentMinute, 0);
                }
                counter.Count++;
                this._Counters[key] = counter;
                if (this._Counters.Count > PruneThreshold)
                {
                    this.PruneStaleWindows(currentMinute);
                }
                return counter.Count > limit;
            }
        }

        private void PruneStaleWindows(long currentMinute)
        {
            List<string> stale = this._Counters.Where(kvp => kvp.Value.WindowMinute < currentMinute).Select(kvp => kvp.Key).ToList();
            foreach (string staleKey in stale)
            {
                this._Counters.Remove(staleKey);
            }
        }

        private struct FixedWindowCounter
        {
            public long WindowMinute;
            public int Count;
            public FixedWindowCounter(long windowMinute, int count)
            {
                this.WindowMinute = windowMinute;
                this.Count = count;
            }
        }
    }
}
