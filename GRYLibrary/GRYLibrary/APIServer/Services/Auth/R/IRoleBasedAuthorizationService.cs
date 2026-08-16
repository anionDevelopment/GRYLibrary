using GRYLibrary.Core.APIServer.Services.Interfaces;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Auth.R
{
    public interface IRoleBasedAuthorizationService : IUserAuthorizationService
    {
        public bool IsAuthorized(ISet<string> groupsOfUser, ISet<string> authorizedGroups);
    }
}
