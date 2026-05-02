using GRYLibrary.Core.APIServer.Mid.M01APIK;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;

namespace GRYLibrary.Core.APIServer.Mid.PreDAPIK
{
    /// <summary>
    /// Represents an <see cref="APIKeyValidatorMiddleware"/> with static (predefined) API-keys which are defined by the configuration.
    /// </summary>
    public class PreDAPIKValidatorMiddleware : APIKeyValidatorMiddleware
    {
        private readonly IPreDAPIKValidatorConfiguration _PreDAPIKValidatorConfiguration;
        public PreDAPIKValidatorMiddleware(RequestDelegate next, IPreDAPIKValidatorConfiguration apiKeyValidatorSettings, IGRYLog log) : base(next, apiKeyValidatorSettings, log)
        {
            this._PreDAPIKValidatorConfiguration = apiKeyValidatorSettings;
        }

        public override bool APIKeyIsAuthorized(string apiKey, HttpContext context)
        {
            return this._PreDAPIKValidatorConfiguration.AuthorizedAPIKeys.Contains(apiKey);
        }
    }
}
