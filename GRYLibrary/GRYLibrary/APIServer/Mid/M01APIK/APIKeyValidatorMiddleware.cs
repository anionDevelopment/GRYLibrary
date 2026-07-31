using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;

namespace GRYLibrary.Core.APIServer.Mid.M01APIK
{
    /// <summary>
    /// Represents an <see cref="AuthorizationMiddleware"/> which implements authorizaton-checks using API-keys.
    /// </summary>
    /// <remarks>
    /// Any valid API-key must have at least 10 characters.
    /// </remarks>
    public abstract class APIKeyValidatorMiddleware : AuthorizationMiddleware
    {
        protected readonly IAPIKeyValidatorConfiguration _APIKeyValidatorSettings;
        private readonly IGRYLog _Log;
        public APIKeyValidatorMiddleware(RequestDelegate next, IAPIKeyValidatorConfiguration apiKeyValidatorSettings, IGRYLog log) : base(next, log, apiKeyValidatorSettings)
        {
            this._APIKeyValidatorSettings = apiKeyValidatorSettings;
            this._Log = log;
        }

        public virtual (bool provided, string apiKey) TryGetAPIKey(HttpContext context)
        {
            return APIKeyValidatorFilter.TryGetAPIKey(context);
        }

        public abstract bool APIKeyIsAuthorized(string apiKey, HttpContext context);
        protected override bool IsAuthorized(HttpContext context)
        {
            bool result;
            (bool provided, string apiKey) = this.TryGetAPIKey(context);
            if (provided)
            {
                context.Items["APIKey"] = apiKey;
                if (apiKey.Length < 10)
                {
                    result = false;
                }
                else
                {
                    result = this.APIKeyIsAuthorized(apiKey, context);
                }
            }
            else
            {
                result = false;
            }
            //The api-key is null when no api-key was provided, so it must not get accessed unconditionally.
            string apiKeyForLog = provided ? $"\"{(5 <= apiKey.Length ? apiKey[..5] : apiKey)}...\"" : "(none)";
            this._Log.Log($"Provided API-Key {apiKeyForLog} is" + (result ? string.Empty : " not") + " valid.", Microsoft.Extensions.Logging.LogLevel.Trace);
            return result;
        }
    }
}