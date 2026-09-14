using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Configuration that tells the authentication-layer which OIDC-providers issue access-tokens that are accepted
    /// on incoming requests. Register an implementation in the dependency-injection-container to let the
    /// authentication-middleware validate OIDC-tokens on subsequent requests (in addition to application-local tokens).
    /// If no implementation is registered, no OIDC-token-validation is performed.
    /// </summary>
    public interface IOIDCAuthenticationConfiguration
    {
        /// <summary>The providers whose access-tokens are accepted on incoming requests.</summary>
        public IList<OIDCProviderConfiguration> Providers { get; set; }
    }
}
