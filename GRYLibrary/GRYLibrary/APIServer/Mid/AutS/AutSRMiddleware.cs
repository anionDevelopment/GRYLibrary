using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.Services.Auth.R;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Mid.AutS
{
    /// <summary>
    /// Represents an <see cref="AuthorizationMiddleware"/> which implements role-based authorizaton-checks using a <see cref="IRoleBasedAuthorizationService"/>.
    /// </summary>
    public class AutSRMiddleware : AuthorizationMiddleware
    {
        private readonly IRoleBasedAuthorizationService _AuthorizationService;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly ICredentialsProvider _CredentialsProvider;
        private readonly IGRYLog _Log;
        public AutSRMiddleware(RequestDelegate next, IRoleBasedAuthorizationService authorizationService, IAuthenticationService authenticationService, ICredentialsProvider credentialsProvider, IAutSRConfiguration authorizationConfiguration, IServerLog log) : base(next, log.Logger, authorizationConfiguration)
        {
            this._AuthorizationService = authorizationService;
            this._AuthenticationService = authenticationService;
            this._CredentialsProvider = credentialsProvider;
            this._Log = log.Logger;
        }


        protected override bool IsAuthorized(HttpContext context)
        {
            if (this.TryGetAuthorizeAttribute(context, out AuthorizeAttribute authorizedAttribute))
            {
                GUtilities.AssertCondition(this._CredentialsProvider.ContainsCredentials(context));
                string accessToken = this._CredentialsProvider.ExtractSecret(context);
                User user = this._AuthenticationService.GetUserByAccessToken(accessToken);
                System.Collections.Generic.ISet<string> authorizedGroups = authorizedAttribute.Groups;
                ISet<Role> userroles = user.GetAllRoles();
                bool result = this._AuthorizationService.IsAuthorized(userroles.Select(r => r.Name).ToHashSet(), authorizedGroups);
                this._Log.Log($"User {user.Id} is" + (result ? string.Empty : " not") + $" authorized. " + "Groups of user: {" + string.Join(", ", userroles.Select(r => r.Name)) + "}; Authorized groups: {" + string.Join(", ", authorizedGroups) + "}", Microsoft.Extensions.Logging.LogLevel.Trace);
                return result;
            }
            else
            {
                throw new InternalAlgorithmException("Error while check authorization information.");
            }
        }
    }
}
