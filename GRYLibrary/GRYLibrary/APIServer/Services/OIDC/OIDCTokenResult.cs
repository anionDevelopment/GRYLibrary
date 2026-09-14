using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>Contains the validated claims extracted from a successful OIDC token exchange.</summary>
    public class OIDCTokenResult
    {
        /// <summary>The subject identifier (sub claim) — uniquely identifies the user at the OIDC provider.</summary>
        public string Subject { get; set; }

        /// <summary>The user's preferred username, if present in the token.</summary>
        public string? PreferredUsername { get; set; }

        /// <summary>The user's email address, if present in the token.</summary>
        public string? Email { get; set; }

        /// <summary>The user's display name, if present in the token.</summary>
        public string? Name { get; set; }

        /// <summary>All claims from the ID token as a flat dictionary.</summary>
        public IDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }
}
