using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OIDC;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace GRYLibrary.Core.APIServer.Mid.AuthS
{
    /// <summary>
    /// Represents an <see cref="AuthenticationMiddleware"/> which implements authentication-checks using <see cref="IAuthenticationService"/>.
    /// In addition to application-local access-tokens it can also accept access-tokens issued by an OIDC-provider:
    /// this is enabled by registering an <see cref="IOIDCService"/> and an <see cref="IOIDCAuthenticationConfiguration"/>
    /// in the dependency-injection-container. If those are not registered, only application-local tokens are accepted.
    /// </summary>
    public class AuthSMiddleware : AuthenticationMiddleware
    {
        private readonly ICredentialsProvider _CredentialsProvider;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly IServiceProvider _ServiceProvider;
        public AuthSMiddleware(RequestDelegate next, IServerLog log, ICredentialsProvider credentialsProvider, IAuthenticationService authenticationService, IAuthSConfiguration authenticationConfiguration, IServiceProvider serviceProvider) : base(next, authenticationConfiguration, authenticationService, log.Logger)
        {
            this._CredentialsProvider = credentialsProvider;
            this._AuthenticationService = authenticationService;
            this._ServiceProvider = serviceProvider;
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
                    if (this.TryGetOIDCAuthentication(accessToken, out principal))
                    {
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

        /// <summary>
        /// Tries to authenticate the request by validating <paramref name="accessToken"/> as an OIDC-token against one of the
        /// configured providers. Returns <see langword="false"/> (without error) if OIDC-token-validation is not configured
        /// or the token is not a valid token of any configured provider.
        /// </summary>
        private bool TryGetOIDCAuthentication(string accessToken, out ClaimsPrincipal? principal)
        {
            principal = null;
            IOIDCService? oidcService = this._ServiceProvider.GetService<IOIDCService>();
            IOIDCAuthenticationConfiguration? oidcConfiguration = this._ServiceProvider.GetService<IOIDCAuthenticationConfiguration>();
            if (oidcService == null || oidcConfiguration == null || oidcConfiguration.Providers == null)
            {
                return false;
            }
            foreach (OIDCProviderConfiguration provider in oidcConfiguration.Providers)
            {
                try
                {
                    OIDCTokenResult result = oidcService.ValidateAccessTokenAsync(provider, accessToken).GetAwaiter().GetResult();
                    principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                        new Claim(ClaimTypes.Name, result.PreferredUsername ?? result.Name ?? result.Subject),
                        new Claim(ClaimTypes.NameIdentifier, result.Subject),
                    }, "OIDC"));
                    return true;
                }
                catch
                {
                    //token is not valid for this provider; try the next one
                }
            }
            return false;
        }
    }
}
