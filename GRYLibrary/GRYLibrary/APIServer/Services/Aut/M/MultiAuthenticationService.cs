using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace GRYLibrary.Core.APIServer.Services.Aut.M
{
    public class MultiAuthenticationService<TUser> : IAuthenticationService<TUser>
        where TUser : User
    {
        private readonly IAuthenticationConfiguration _AuthenticationConfiguration;

        public MultiAuthenticationService(IAuthenticationConfiguration authenticationConfiguration)
        {
            this._AuthenticationConfiguration = authenticationConfiguration;
        }

        public bool AccessTokenIsValid(string accessToken)
        {
            throw new NotImplementedException();
        }

        public void AddRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public void AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public void AddUserTyped(TUser user)
        {
            throw new NotImplementedException();
        }

        public void EnsureRoleDoesNotExist(string roleName)
        {
            throw new NotImplementedException();
        }

        public void EnsureRoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public void EnsureUserDoesNotHaveRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public void EnsureUserHasRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public ISet<User> GetAllUser()
        {
            throw new NotImplementedException();
        }

        public ISet<TUser> GetAllUserTyped()
        {
            throw new NotImplementedException();
        }

        public string GetBaseRoleOfAllUser()
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal GetPrincipal(string accessToken)
        {
            throw new NotImplementedException();
        }

        public Role GetRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }

        public ISet<string> GetRolesOfUser(string userId)
        {
            throw new NotImplementedException();
        }

        public User GetUser(string userId)
        {
            throw new NotImplementedException();
        }

        public User GetUserByAccessToken(string accessToken)
        {
            throw new NotImplementedException();
        }

        public TUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public User GetUserByName(string name)
        {
            throw new NotImplementedException();
        }

        public TUser GetUserByNameTyped(string name)
        {
            throw new NotImplementedException();
        }

        public string GetUserName(string accessToken)
        {
            throw new NotImplementedException();
        }

        public TUser GetUserTyped(string userId)
        {
            throw new NotImplementedException();
        }

        public string Hash(string password)
        {
            throw new NotImplementedException();
        }

        public AccessToken Login(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public void Logout(string accessToken)
        {
            throw new NotImplementedException();
        }

        public void Logout(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public void LogoutEverywhere(string userId)
        {
            throw new NotImplementedException();
        }

        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        public bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public void UpdateRole(Role role)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(TUser user)
        {
            throw new NotImplementedException();
        }

        public bool UserExists(string userId)
        {
            throw new NotImplementedException();
        }

        public bool UserExistsByName(string userNameAdmin)
        {
            throw new NotImplementedException();
        }

        public bool UserHasRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public bool UserWithNameExists(string username)
        {
            throw new NotImplementedException();
        }
    }
}
