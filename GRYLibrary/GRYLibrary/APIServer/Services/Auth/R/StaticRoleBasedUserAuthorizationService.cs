using System.Collections.Generic;
using System.Linq;
using User = GRYLibrary.Core.APIServer.CommonDBTypes.User;

namespace GRYLibrary.Core.APIServer.Services.Auth.R
{
    /// <summary>
    /// This is a transient <see cref="IRoleBasedAuthorizationService"/>.
    /// </summary>
    public class StaticRoleBasedUserAuthorizationService<UserType> : IRoleBasedAuthorizationService
        where UserType : User
    {
        public StaticRoleBasedUserAuthorizationService()
        {
        }

        public bool IsAuthorized(ISet<string> groupsOfUser, ISet<string> authorizedGroups)
        {
            bool result = groupsOfUser.Intersect(authorizedGroups).Any();
            return result;
        }
    }
}
