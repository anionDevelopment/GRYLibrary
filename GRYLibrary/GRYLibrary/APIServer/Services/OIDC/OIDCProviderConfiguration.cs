namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>Holds the configuration for a single OpenID Connect provider.</summary>
    public class OIDCProviderConfiguration
    {
        /// <summary>Unique identifier for this provider entry (e.g. "keycloak-1").</summary>
        public string Id { get; set; }

        /// <summary>Human-readable label shown on the login button (e.g. "Keycloak").</summary>
        public string DisplayName { get; set; }

        /// <summary>OIDC authority base URL (e.g. "https://keycloak.example.com/realms/myrealm").</summary>
        public string Authority { get; set; }

        /// <summary>The client_id registered in the OIDC provider.</summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The client_secret of a confidential client, if the provider requires client-authentication.
        /// Leave <see langword="null"/> or empty for a public client (which relies on PKCE instead).
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// The space-separated OIDC-scopes to request. If <see langword="null"/> or empty, "openid profile email" is used.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// The audience that an incoming access-token must contain when it is validated on subsequent requests
        /// (see <see cref="IOIDCService.ValidateAccessTokenAsync"/>). If <see langword="null"/> or empty,
        /// the audience of incoming access-tokens is not validated (only issuer, signature and lifetime are checked).
        /// This does not affect id-token-validation, whose audience is always the <see cref="ClientId"/>.
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        /// The redirect URI that will receive the authorization code.
        /// Must be configured identically in the OIDC provider.
        /// This should point to the frontend callback page, e.g. "https://opendms.example.com/oidc-callback".
        /// Only relevant for the browser-based authorization-code-flow; not used by the password-flow.
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
