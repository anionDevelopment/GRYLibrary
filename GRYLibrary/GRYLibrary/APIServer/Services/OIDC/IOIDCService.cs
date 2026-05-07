using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Injectable service that encapsulates the OpenID Connect Authorization Code flow with PKCE.
    /// Implement this interface and register it with DI to enable reusable OIDC login in any service.
    /// </summary>
    public interface IOIDCService
    {
        /// <summary>
        /// Initiates an OIDC login by fetching the discovery document and building the authorization URL.
        /// Store the returned <see cref="OIDCAuthorizationRequest.State"/> and
        /// <see cref="OIDCAuthorizationRequest.CodeVerifier"/> server-side, keyed by state.
        /// Redirect the user-agent to <see cref="OIDCAuthorizationRequest.AuthorizationUrl"/>.
        /// </summary>
        Task<OIDCAuthorizationRequest> InitiateLoginAsync(OIDCProviderConfiguration provider);

        /// <summary>
        /// Exchanges the authorization code for tokens, validates the ID token, and returns its claims.
        /// </summary>
        /// <param name="provider">The provider configuration that was used during initiation.</param>
        /// <param name="code">The authorization code received from the OIDC provider callback.</param>
        /// <param name="codeVerifier">The PKCE code verifier that was generated during initiation.</param>
        Task<OIDCTokenResult> ExchangeCodeAsync(OIDCProviderConfiguration provider, string code, string codeVerifier);
    }
}
