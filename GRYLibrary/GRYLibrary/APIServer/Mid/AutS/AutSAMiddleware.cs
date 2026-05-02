using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.Services.Auth.A;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Mid.AutS
{
    /// <summary>
    /// Represents an <see cref="AuthorizationMiddleware"/> which implements action-based authorizaton-checks using a <see cref="IActionBasedAuthorizationService"/>.
    /// </summary>
    public class AutSAMiddleware : AuthorizationMiddleware
    {
        private readonly IActionBasedAuthorizationService _AuthorizationService;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly ICredentialsProvider _CredentialsProvider;
        public AutSAMiddleware(RequestDelegate next, IActionBasedAuthorizationService authorizationService, IAuthenticationService authenticationService, ICredentialsProvider credentialsProvider, IAuthorizationConfiguration authorizationConfiguration, IGRYLog log) : base(next, log, authorizationConfiguration)
        {
            this._AuthorizationService = authorizationService;
            this._AuthenticationService = authenticationService;
            this._CredentialsProvider = credentialsProvider;
        }
        protected override bool IsAuthorized(HttpContext context)
        {
            if (this.TryGetActionAttribute(context, out ActionAttribute actionAttribute))
            {
                GUtilities.AssertCondition(this._CredentialsProvider.ContainsCredentials(context));
                string accessToken = this._CredentialsProvider.ExtractSecret(context);
                User user = this._AuthenticationService.GetUserByAccessToken(accessToken);
                return this._AuthorizationService.IsAuthorized(user.Id, actionAttribute.Action);
            }
            else
            {
                throw new InternalAlgorithmException("Error while loading authorization information.");
            }
        }
    }
}
