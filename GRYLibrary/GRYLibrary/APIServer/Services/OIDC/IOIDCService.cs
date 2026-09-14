using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Injectable service that encapsulates OpenID Connect login-flows against any OIDC-compliant provider (e.g. Keycloak).
    /// Register it with DI to enable reusable OIDC-login in any service.
    /// It supports the recommended browser-based authorization-code-flow with PKCE
    /// (<see cref="InitiateLoginAsync"/> + <see cref="ExchangeCodeAsync"/>) as well as the password-flow
    /// (<see cref="LoginWithPasswordAsync"/>) which lets a service delegate a classic username/password-login to the provider.
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

        /// <summary>
        /// Authenticates a user directly with username and password against the OIDC-provider using the
        /// resource-owner-password-credentials-flow ("password grant") and returns the resulting tokens.
        /// This lets an application delegate its classic username/password-login to the provider:
        /// the application forwards the credentials it received and returns the resulting access-token to the client.
        /// The provider's client must have the direct-access-grant ("Direct Access Grants") enabled.
        /// Throws an <see cref="System.InvalidOperationException"/> if the credentials are rejected by the provider.
        /// </summary>
        /// <param name="provider">The provider configuration to authenticate against.</param>
        /// <param name="username">The end-user's username.</param>
        /// <param name="password">The end-user's plain-text password.</param>
        Task<OIDCPasswordLoginResult> LoginWithPasswordAsync(OIDCProviderConfiguration provider, string username, string password);

        /// <summary>
        /// Validates an access-token (a JWT that was issued by the provider) and returns its claims.
        /// Use this to authenticate subsequent requests that carry an OIDC-token instead of an application-local token:
        /// the token's signature is verified against the provider's published keys (JWKS) and its issuer and lifetime are checked.
        /// The audience is only checked if <see cref="OIDCProviderConfiguration.Audience"/> is set.
        /// Throws an <see cref="System.Exception"/> (e.g. <see cref="Microsoft.IdentityModel.Tokens.SecurityTokenException"/>)
        /// if the token is invalid, expired or not issued by the provider.
        /// </summary>
        /// <param name="provider">The provider configuration the token is expected to originate from.</param>
        /// <param name="accessToken">The raw JWT to validate.</param>
        Task<OIDCTokenResult> ValidateAccessTokenAsync(OIDCProviderConfiguration provider, string accessToken);
    }
}
