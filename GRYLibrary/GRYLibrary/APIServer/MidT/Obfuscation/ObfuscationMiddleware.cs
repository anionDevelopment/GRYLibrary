using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Settings;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.Obfuscation
{
    /// <summary>
    /// Represents a middleware which normalizes the response-status-code in the <see cref="Productive"/>-environment
    /// to avoid leaking information through fine-grained status-codes:
    /// every 2xx-response is reported as 200 and every 4xx- or 5xx-response is reported as 400.
    /// No other information (headers, body, other status-code-ranges) is changed.
    /// </summary>
    public abstract class ObfuscationMiddleware : AbstractMiddleware
    {
        private readonly IApplicationConstants _AppConstants;
        /// <inheritdoc/>
        public ObfuscationMiddleware(RequestDelegate next, IApplicationConstants appConstants) : base(next)
        {
            this._AppConstants = appConstants;
        }
        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            if (this._AppConstants.Environment is Productive)
            {
                // The final status-code is not known yet at this point and the response may be buffered by an
                // inner middleware, so the normalization is registered as an OnStarting-callback which runs
                // right before the response is actually sent.
                context.Response.OnStarting(() =>
                {
                    int statusCodeCategory = context.Response.StatusCode / 100;
                    if (statusCodeCategory == 2)
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                    }
                    else if (statusCodeCategory == 4 || statusCodeCategory == 5)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    }
                    return Task.CompletedTask;
                });
            }
            return this._Next(context);
        }
    }
}
