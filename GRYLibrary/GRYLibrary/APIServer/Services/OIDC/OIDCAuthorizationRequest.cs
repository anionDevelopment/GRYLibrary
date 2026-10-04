namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>Holds all data produced when initiating an OIDC authorization code flow.</summary>
    public class OIDCAuthorizationRequest
    {
        /// <summary>The full URL to redirect the user-agent to at the OIDC provider.</summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>The opaque state value included in the authorization URL. Must be verified on callback.</summary>
        public string State { get; set; }

        /// <summary>The PKCE code verifier. Keep server-side; do not expose to the client.</summary>
        public string CodeVerifier { get; set; }

        /// <summary>
        /// The nonce included in the authorization URL. Keep it server-side next to the <see cref="CodeVerifier"/> and,
        /// on callback, compare it against the <c>nonce</c>-claim of the id-token (available in
        /// <see cref="OIDCTokenResult.Claims"/>), the same way the <see cref="State"/> is verified.
        /// </summary>
        public string Nonce { get; set; }
    }
}
