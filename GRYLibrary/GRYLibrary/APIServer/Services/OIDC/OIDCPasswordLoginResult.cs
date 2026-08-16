using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Contains the tokens and validated claims produced by a successful password-based OIDC-login
    /// (resource-owner-password-credentials-flow).
    /// </summary>
    public class OIDCPasswordLoginResult
    {
        /// <summary>The access-token issued by the OIDC-provider. Return this to the client.</summary>
        public string AccessToken { get; set; }

        /// <summary>The token-type of <see cref="AccessToken"/> (usually "Bearer").</summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>The refresh-token, if the provider issued one; otherwise <see langword="null"/>.</summary>
        public string? RefreshToken { get; set; }

        /// <summary>The id-token, if the provider issued one (requires the "openid" scope); otherwise <see langword="null"/>.</summary>
        public string? IdToken { get; set; }

        /// <summary>The lifetime of <see cref="AccessToken"/> in seconds, or 0 if the provider did not report it.</summary>
        public int ExpiresInSeconds { get; set; }

        /// <summary>The subject identifier (sub claim) of the id-token, if an id-token was returned and validated.</summary>
        public string? Subject { get; set; }

        /// <summary>The user's preferred username, if present in the id-token.</summary>
        public string? PreferredUsername { get; set; }

        /// <summary>The user's email address, if present in the id-token.</summary>
        public string? Email { get; set; }

        /// <summary>The user's display name, if present in the id-token.</summary>
        public string? Name { get; set; }

        /// <summary>All claims from the validated id-token as a flat dictionary. Empty if no id-token was returned.</summary>
        public IDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }
}
