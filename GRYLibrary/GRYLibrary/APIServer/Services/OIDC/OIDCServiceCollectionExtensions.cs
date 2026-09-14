using Microsoft.Extensions.DependencyInjection;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Extension-methods to register the OIDC-services in the dependency-injection-container.
    /// </summary>
    public static class OIDCServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default <see cref="IOIDCService"/> implementation as a singleton.
        /// Call this from an application that wants to offer OIDC-login. The application still supplies the
        /// <see cref="OIDCProviderConfiguration"/> (e.g. its Keycloak-settings) and calls the service where needed
        /// (for example from within its own login-implementation to delegate the login to the provider).
        /// </summary>
        public static IServiceCollection AddOIDC(this IServiceCollection services)
        {
            services.AddSingleton<IOIDCService, OIDCService>();
            return services;
        }
    }
}
