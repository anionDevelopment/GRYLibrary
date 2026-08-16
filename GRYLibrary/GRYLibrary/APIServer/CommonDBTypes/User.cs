using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.MFA;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.CommonDBTypes
{
    [PrimaryKey(nameof(Id))]
    public class User : IEquatable<User>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? PasswordHash { get; set; } = default;
        public string? EMailAddress { get; set; } = default;
        public bool UserIsActivated { get; set; } = true;
        public bool UserIsLocked { get; set; } = false;
        public DateTimeOffset RegistrationMoment { get; set; } = default;
        public HashSet<RefreshToken> RefreshToken { get; set; } = [];
        public HashSet<AccessToken> AccessToken { get; set; } = [];
        public HashSet<Role> Roles { get; set; } = [];
        public TOTP? TOTP { get; set; } = default;

        public User()
        {

        }

        public static User CreateNewUser(string username, string passwordHash, ITimeService timeService)
        {
            return CreateNewUser(new User(), username, passwordHash, timeService);
        }

        public static T CreateNewUser<T>(T resultObject, string username, string passwordHash, ITimeService timeService)
            where T : User
        {
            T user = resultObject;
            user.Id = Guid.NewGuid().ToString();
            user.Name = username;
            user.EMailAddress = null;
            user.PasswordHash = passwordHash;
            user.RegistrationMoment = timeService.GetCurrentLocalTimeAsDateTimeOffset();
            user.RefreshToken = [];
            user.AccessToken = [];
            user.Roles = [];
            user.TOTP = new GRYLibrary.Core.APIServer.MFA.TOTP() { IsActicated = false, SecretKey = Guid.NewGuid().ToString("N") };
            return user;
        }
        public ISet<Role> GetAllRoles()
        {
            ISet<Role> result = new HashSet<Role>();
            foreach (Role role in this.Roles)
            {
                result.Add(role);
                result.UnionWith(role.GetAllInheritedRoles());
            }
            return result;
        }
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as User);
        }

        public virtual bool Equals(User? other)
        {
            return other is not null &&
                   this.Id == other.Id &&
                   this.Name == other.Name &&
                   this.EMailAddress == other.EMailAddress &&
                   this.PasswordHash == other.PasswordHash &&
                   this.UserIsActivated == other.UserIsActivated &&
                   this.UserIsLocked == other.UserIsLocked &&
                   this.RefreshToken.SetEquals(other.RefreshToken) &&
                   this.AccessToken.SetEquals(other.AccessToken);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.Id);
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"User \"{this.Name}\" (Id: {this.Id})";
        }
    }
}
