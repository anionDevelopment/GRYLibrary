using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Trans
{
    /// Represents a authenticationservice-persistence where userdata (user, roles, accesstoken, etc.) will be stored.
    public interface IAuthenticationServicePersistence<UserType>
        where UserType : User
    {
        public IDictionary<string, UserType> GetAllUsers();
        public ISet<Role> GetAllRoles();
        public void AddRole(Role role);
        public void UpdateRole(Role role);
        public void DeleteRoleByName(string roleName);
        public bool AccessTokenExists(string accessToken, out UserType? user);
        public void AddUser(UserType newUser);
        public bool UserWithNameExists(string userName);
        public bool UserWithIdExists(string userId);
        public UserType GetUserById(string userId);
        public UserType GetUserByName(string userName);
        public void RemoveUser(string userId);
        public bool RoleExists(string roleName);
        public void AddRoleToUser(string userId, string roleId);
        public void RemoveRoleFromUser(string userId, string roleId);
        public bool UserHasRole(string userId, string roleId);
        public UserType GetUserByAccessToken(string accessToken);
        public void UpdateUser(UserType user);
        public AccessToken GetAccessToken(string accessToken);
        public void AddAccessToken(AccessToken newAccessToken);
        public void RemoveAccessToken(string accessToken);
        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId);
        public Role GetRoleByName(string roleName);
        public Role GetRoleById(string roleId);
    }
}
