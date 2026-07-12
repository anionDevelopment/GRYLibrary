using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Auth.A
{
    public interface IStaticActionBasedUserAuthorizationServiceConfiguration
    {
        /// <summary>
        /// Key: Action
        /// Value: Authorized groups
        /// </summary>
        public Dictionary<string, ISet<string>> AuthorizedGroups { get; set; }
    }
}
