using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.MidT.Aut
{
    public class AuthorizationConfiguration : IAuthorizationConfiguration
    {
        public bool Enabled { get; set; } = true;

        public virtual ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>();
        }
    }
}