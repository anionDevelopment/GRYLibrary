using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>Default implementation of <see cref="IOIDCAuthenticationConfiguration"/>.</summary>
    public class OIDCAuthenticationConfiguration : IOIDCAuthenticationConfiguration
    {
        /// <inheritdoc/>
        public IList<OIDCProviderConfiguration> Providers { get; set; } = new List<OIDCProviderConfiguration>();
    }
}
