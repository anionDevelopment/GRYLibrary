using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.WAF
{
    /// <summary>
    /// Represents a web-application-firewall.
    /// This abstract middleware provides the mechanism to inspect an incoming request and to block and log
    /// requests which are classified as malicious. The concrete rules (which requests are considered
    /// malicious, e.g. suspicious content in the route or payload, invalid or entity-expanding XML/JSON,
    /// oversized payloads, route-specific exceptions, etc.) must be provided by a concrete implementation
    /// in <see cref="CheckRequest"/>.
    /// </summary>
    public abstract class WebApplicationFirewallMiddleware : AbstractMiddleware
    {
        private readonly IGRYLog _Log;
        /// <inheritdoc/>
        public WebApplicationFirewallMiddleware(RequestDelegate next, IGRYLog log) : base(next)
        {
            this._Log = log;
        }

        /// <summary>
        /// Evaluates whether the given request is allowed to pass the firewall.
        /// A concrete implementation provides the actual firewall-rules here and returns
        /// <see cref="WebApplicationFirewallResult.Allow"/> or <see cref="WebApplicationFirewallResult.Block"/>.
        /// The request-body can be read (without consuming the request-stream for the downstream pipeline)
        /// using <see cref="GetRequestBody"/>.
        /// </summary>
        protected abstract WebApplicationFirewallResult CheckRequest(HttpContext context);

        /// <summary>
        /// Reads the request-body as a byte-array and re-buffers it so that the downstream pipeline can read
        /// it again. Intended to be used by concrete firewall-rules that need to inspect the payload.
        /// </summary>
        protected byte[] GetRequestBody(HttpContext context)
        {
            return Tools.GetRequestBody(context.Request);
        }

        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            WebApplicationFirewallResult result = this.CheckRequest(context);
            if (result.RequestIsAllowed)
            {
                return this._Next(context);
            }
            this._Log.Log($"The web-application-firewall blocked a request to \"{context.Request.Path}\"{(string.IsNullOrEmpty(result.Reason) ? string.Empty : $" (reason: {result.Reason})")}.", LogLevel.Warning);
            context.Response.StatusCode = result.StatusCode;
            return Task.CompletedTask;
        }
    }
}
