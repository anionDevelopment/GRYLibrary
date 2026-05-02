using GRYLibrary.Core.APIServer.Services.Aut.Prov;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.MidT.Auth
{
    public interface IAuthenticationConfiguration : IMiddlewareConfiguration
    {
        public ISet<string> RoutesWhereUnauthenticatedAccessIsAllowed { get; set; }
        /// <summary>
        /// Key: Authentication-provider-name
        /// </summary>
        public IDictionary<string, IAuthenticationProviderConfiguration> AuthentificationMethods { get; set; }
    }
}
