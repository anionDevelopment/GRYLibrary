using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Auth.A
{
    public class StaticActionBasedUserAuthorizationServiceConfiguration : IStaticActionBasedUserAuthorizationServiceConfiguration
    {
        public Dictionary<string, ISet<string>> AuthorizedGroups { get; set; }
    }
}
