using GRYLibrary.Core.APIServer.Services.Aut.Prov;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.MidT.Auth
{
    public class AuthenticationConfiguration : IAuthenticationConfiguration
    {
        public bool Enabled { get; set; } = true;
        public ISet<string> RoutesWhereUnauthenticatedAccessIsAllowed { get; set; } = new HashSet<string>();

        public IDictionary<string, IAuthenticationProviderConfiguration> AuthentificationMethods { get; set; } = new Dictionary<string, IAuthenticationProviderConfiguration>();

        public virtual ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>();
        }
    }
}