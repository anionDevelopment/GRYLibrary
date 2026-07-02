using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Crypto;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AccessToken = GRYLibrary.Core.APIServer.CommonAuthenticationTypes.AccessToken;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using User = GRYLibrary.Core.APIServer.CommonDBTypes.User;

namespace GRYLibrary.Core.APIServer.Services.Trans
{
    /// <summary>
    /// This is a transient <see cref="IAuthenticationService{UserType}"/> for testing purposes.
    /// </summary>
    /// <remarks>
    /// Do not use this service in productive-mode because this service does not implement many features to increase security.
    /// </remarks>
    public class TransientAuthenticationService<UserType> : IAuthenticationService<UserType>
        where UserType : User
    {

        private readonly ITimeService _TimeService;
        private readonly ITransientAuthenticationServicePersistence<UserType> _TransientAuthenticationServicePersistence;
        private readonly IAuthenticationServiceSettings _AuthenticationServiceSettings;
        public TransientAuthenticationService(ITimeService timeService, ITransientAuthenticationServicePersistence<UserType> transientAuthenticationServicePersistence, IAuthenticationServiceSettings authenticationServiceSettings)
        {
            this._TimeService = timeService;
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._AuthenticationServiceSettings = authenticationServiceSettings;
        }

        public virtual string Hash(string password)
        {
            string result = GUtilities.ByteArrayToHexString(new SHA256().Hash(GUtilities.StringToByteArray(password)));
            return result;
        }

        public virtual AccessToken Login(string userName, string password)
        {
            if (!this._TransientAuthenticationServicePersistence.UserWithNameExists(userName))
            {
                return this.ThrowInvalidCredentialsException();
            }
            this._TransientAuthenticationServicePersistence.GetUserByName(userName);
            UserType user = this.GetUserByNameTyped(userName);
            if (this.Hash(password) != user.PasswordHash)
            {
                return this.ThrowInvalidCredentialsException();
            }
            if (user.UserIsLocked)
            {
                throw new NotAuthorizedException($"User '{userName}' is locked.");
            }
            AccessToken newAccessToken = new AccessToken();
            newAccessToken.Value = Guid.NewGuid().ToString();
            newAccessToken.OwnerUserId = user.Id;
            newAccessToken.ExpiredMoment = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1);//TODO make this configurable
            this._TransientAuthenticationServicePersistence.AddAccessToken(newAccessToken);
            user.AccessToken.Add(newAccessToken);
            return newAccessToken;
        }

        private AccessToken ThrowInvalidCredentialsException()
        {
            throw new BadRequestException(StatusCodes.Status400BadRequest, "Invalid credentials");
        }

        public virtual void Logout(string accessToken)
        {
            if (this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out UserType user))
            {
                user.AccessToken = [.. user.AccessToken.Where(at => at.Value != accessToken)];
            }
            else
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "Accesstoken not found.");
            }
        }
        public virtual void LogoutEverywhere(string userId)
        {
            UserType user = this._TransientAuthenticationServicePersistence.GetUserById(userId);
            user.RefreshToken.Clear();
            user.AccessToken.Clear();
        }

        public virtual bool UserExists(string userId)
        {
            return this._TransientAuthenticationServicePersistence.UserWithIdExists(userId);
        }

        public virtual bool AccessTokenIsValid(string accessToken)
        {

            if (this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out UserType user))
            {
                AccessToken at = user.AccessToken.Where(at => at.Value == accessToken).First();
                return this._TimeService.GetCurrentTimeInUTCAsDateTimeOffset() < at.ExpiredMoment;
            }
            else
            {
                return false;
            }
        }

        public virtual ISet<UserType> GetAllUserTyped()
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers().Values.ToHashSet();
        }

        public virtual UserType GetUserTyped(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        public virtual string GetUserName(string accessToken)
        {
            if (this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out UserType user))
            {
                return user.Name;
            }
            else
            {
                throw new BadRequestException(400, "Accesstoken not found");
            }
        }

        public virtual void RemoveUser(string userId)
        {
            this._TransientAuthenticationServicePersistence.RemoveUser(userId);
        }

        public virtual ISet<User> GetAllUser()
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers().Values.Cast<User>().ToHashSet();
        }

        public virtual User GetUser(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        public virtual Role GetRoleByName(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles().Where(r => r.Name == roleName).First();
        }

        public virtual Role GetRoleById(string roleId)
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles().Where(r => r.Id == roleId).First();
        }

        public virtual void EnsureUserHasRole(string userId, string roleId)
        {
            Role role = this.GetRoleById(roleId);
            UserType user = this._TransientAuthenticationServicePersistence.GetAllUsers()[userId];
            user.Roles.Add(role);
        }

        public virtual void EnsureUserDoesNotHaveRole(string userId, string roleId)
        {
            Role role = this.GetRoleById(roleId);
            UserType user = this._TransientAuthenticationServicePersistence.GetAllUsers()[userId];
            user.Roles.Remove(role);
        }

        public virtual bool UserHasRole(string userId, string roleId)
        {
            Role role = this.GetRoleById(roleId);
            UserType user = this._TransientAuthenticationServicePersistence.GetAllUsers()[userId];
            return user.Roles.Contains(role);
        }

        public virtual bool RoleExists(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles().Where(r => r.Name == roleName).Any();
        }

        public virtual void EnsureRoleExists(string roleName)
        {
            if (!this.RoleExists(roleName))
            {
                Role newRole = new Role();
                newRole.Id = Guid.NewGuid().ToString();
                newRole.Name = roleName;
                newRole.DirectlyInheritedRoles = [];
                this._TransientAuthenticationServicePersistence.AddRole(newRole);
            }
        }

        public virtual void EnsureRoleDoesNotExist(string roleName)
        {
            if (this.RoleExists(roleName))
            {
                this._TransientAuthenticationServicePersistence.DeleteRoleByName(roleName);
            }
        }
        public virtual ISet<string> GetRolesOfUser(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId).Roles.SelectMany(r => r.DirectlyInheritedRoles.Select(s => s.Name).Union(new HashSet<string>() { r.Name })).ToHashSet();
        }

        public virtual void AddUserTyped(UserType user)
        {
            if (this._TransientAuthenticationServicePersistence.UserWithNameExists(user.Name))
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "User with this name already exists.");
            }
            this._TransientAuthenticationServicePersistence.AddUser(user);
        }

        public virtual void AddUser(User user)
        {
            this.AddUserTyped((UserType)user);
        }

        public virtual UserType GetUserByNameTyped(string username)
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers().Where(kvp => kvp.Value.Name == username).First().Value;
        }

        public virtual User GetUserByName(string name)
        {
            return this.GetUserByNameTyped(name);
        }

        public virtual bool UserWithNameExists(string username)
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers().Where(kvp => kvp.Value.Name == username).Any();
        }

        public virtual UserType GetUserById(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        public virtual User GetUserByAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByAccessToken(accessToken);
        }

        public virtual void AddRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public virtual bool UserExistsByName(string username)
        {
            return this._TransientAuthenticationServicePersistence.UserWithNameExists(username);
        }

        public virtual void UpdateRole(Role role)
        {
            this._TransientAuthenticationServicePersistence.UpdateRole(role);
        }

        public virtual void Logout(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public virtual ISet<Role> GetRoles(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(UserType user)
        {
            this._TransientAuthenticationServicePersistence.UpdateUser(user);
        }

        public string GetBaseRoleOfAllUser()
        {
            return this._AuthenticationServiceSettings.BaseRoleOfAllUser;
        }

        public ClaimsPrincipal GetPrincipal(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
