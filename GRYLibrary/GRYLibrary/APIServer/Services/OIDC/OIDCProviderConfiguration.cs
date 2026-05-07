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
        /// The redirect URI that will receive the authorization code.
        /// Must be configured identically in the OIDC provider.
        /// This should point to the frontend callback page, e.g. "https://opendms.example.com/oidc-callback".
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
