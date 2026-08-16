using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;

namespace GRYLibrary.Core.APIServer.Mid.AuthS
{
    /// <summary>
    /// Represents an <see cref="AuthenticationMiddleware"/> which implements authentication-checks using <see cref="IAuthenticationService"/>.
    /// </summary>
    public class AuthSMiddleware : AuthenticationMiddleware
    {
        private readonly ICredentialsProvider _CredentialsProvider;
        private readonly IAuthenticationService _AuthenticationService;
        public AuthSMiddleware(RequestDelegate next, IServerLog log, ICredentialsProvider credentialsProvider, IAuthenticationService authenticationService, IAuthSConfiguration authenticationConfiguration) : base(next, authenticationConfiguration, authenticationService, log.Logger)
        {
            this._CredentialsProvider = credentialsProvider;
            this._AuthenticationService = authenticationService;
        }

        public override bool TryGetAuthentication(HttpContext context, out ClaimsPrincipal? principal, out string? accessToken)
        {
            try
            {
                if (this._CredentialsProvider.ContainsCredentials(context))
                {
                    accessToken = this._CredentialsProvider.ExtractSecret(context);
                    if (_AuthenticationService.AccessTokenIsValid(accessToken))
                    {
                        User user = _AuthenticationService.GetUserByAccessToken(accessToken);
                        principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                        }, "Basic"));
                        return true;
                    }
                }
            }
            catch
            {
                //ignore errors, just return false
            }
            principal = null;
            accessToken = null;
            return false;

        }
    }
}
