using Microsoft.AspNetCore.Http;

namespace GRYLibrary.Core.APIServer.MidT.WAF
{
    /// <summary>
    /// Represents the result of a <see cref="WebApplicationFirewallMiddleware"/>-evaluation of a request:
    /// either the request is allowed to pass or it is blocked with a specific HTTP-status-code and an
    /// optional reason which is written to the log.
    /// </summary>
    public sealed class WebApplicationFirewallResult
    {
        /// <summary>Whether the request is allowed to pass the firewall.</summary>
        public bool RequestIsAllowed { get; }
        /// <summary>The HTTP-status-code returned to the client when the request is blocked.</summary>
        public int StatusCode { get; }
        /// <summary>An optional reason for blocking the request. It is written to the log but not returned to the client.</summary>
        public string Reason { get; }

        private WebApplicationFirewallResult(bool requestIsAllowed, int statusCode, string reason)
        {
            this.RequestIsAllowed = requestIsAllowed;
            this.StatusCode = statusCode;
            this.Reason = reason;
        }

        /// <summary>Allows the request to pass the firewall.</summary>
        public static WebApplicationFirewallResult Allow()
        {
            return new WebApplicationFirewallResult(true, 0, null);
        }

        /// <summary>
        /// Blocks the request. The request is answered with the given <paramref name="statusCode"/>
        /// (default: 403) and the given <paramref name="reason"/> is written to the log.
        /// </summary>
        public static WebApplicationFirewallResult Block(string reason = null, int statusCode = StatusCodes.Status403Forbidden)
        {
            return new WebApplicationFirewallResult(false, statusCode, reason);
        }
    }
}
