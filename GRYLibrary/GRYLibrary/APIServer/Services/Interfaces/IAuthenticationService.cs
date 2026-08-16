using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using System.Collections.Generic;
using System.Security.Claims;

namespace GRYLibrary.Core.APIServer.Services.Interfaces
{
    /// <summary>
    /// Represents a service which manages users and its roles.
    /// </summary>
    public interface IAuthenticationService
    {
        public string Hash(string password);
        public void AddUser(User user);
        public AccessToken Login(string userName, string password);
        public bool AccessTokenIsValid(string accessToken);
        public ClaimsPrincipal GetPrincipal(string accessToken);
        /// <remarks>
        /// This operation does not check if the <paramref name="accessToken"/> is valid.
        /// </remarks>
        public string GetUserName(string accessToken);
        public void RemoveUser(string userId);
        bool UserExists(string userId);
        public ISet<User> GetAllUser();
        public User GetUser(string userId);
        #region Roles
        public void AddRole(string roleName);
        public void EnsureUserHasRole(string userId, string roleId);
        public void EnsureUserDoesNotHaveRole(string userId, string roleId);
        public bool UserHasRole(string userId, string roleId);
        public bool RoleExists(string roleName);
        public void EnsureRoleExists(string roleName);
        public void EnsureRoleDoesNotExist(string roleName);
        public Role GetRoleByName(string roleName);
        /// <returns>Returns all roles of a user including inherited roles</returns>
        public ISet<string> GetRolesOfUser(string userId);
        #endregion
        void Logout(string accessToken);
        /// <remarks>
        /// Logs the user everywhere out.
        /// </remarks>
        void Logout(ClaimsPrincipal user);
        void LogoutEverywhere(string userId);
        public User GetUserByName(string name);
        User GetUserByAccessToken(string accessToken);
        bool UserExistsByName(string userNameAdmin);
        void UpdateRole(Role role);
        string GetBaseRoleOfAllUser();
    }
    /// <summary>
    /// Represents a authentication-service with a custom user-type.
    /// </summary>
    /// <typeparam name="UserType"></typeparam>
    public interface IAuthenticationService<UserType> : IAuthenticationService
    where UserType : User
    {
        public ISet<UserType> GetAllUserTyped();
        public UserType GetUserTyped(string userId);
        public void AddUserTyped(UserType user);
        public UserType GetUserByNameTyped(string name);
        bool UserWithNameExists(string username);
        UserType GetUserById(string userId);
        void UpdateUser(UserType user);
    }
}
