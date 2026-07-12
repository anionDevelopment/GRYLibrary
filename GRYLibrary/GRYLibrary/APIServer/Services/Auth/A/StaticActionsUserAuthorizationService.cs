using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Logging.GRYLogger;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.APIServer.Services.Auth.A
{
    /// <summary>
    /// This is a transient <see cref="IUserAuthorizationService"/> for testing purposes.
    /// </summary>
    public class StaticActionsUserAuthorizationService<UserType> : IActionBasedAuthorizationService
        where UserType : User
    {
        private readonly IAuthenticationService<UserType> _AuthenticationService;
        private readonly IStaticActionBasedUserAuthorizationServiceConfiguration _StaticGroupUserAuthorizationServiceConfiguration;
        private readonly IGRYLog _Log;
        public StaticActionsUserAuthorizationService(IAuthenticationService<UserType> authenticationService, IStaticActionBasedUserAuthorizationServiceConfiguration staticGroupUserAuthorizationServiceConfiguration, IGRYLog log)
        {
            this._AuthenticationService = authenticationService;
            this._StaticGroupUserAuthorizationServiceConfiguration = staticGroupUserAuthorizationServiceConfiguration;
            this._Log = log;
        }

        public bool IsAuthorized(string userId, string action)
        {
            ISet<string> groupsOfUser = this._AuthenticationService.GetRolesOfUser(userId);
            bool result = groupsOfUser.Intersect(this._StaticGroupUserAuthorizationServiceConfiguration.AuthorizedGroups[action]).Any();
            this._Log.Log($"User {userId} is" + (result ? string.Empty : " not") + $" authorized for action {action}. " + "Groups of user: {" + string.Join(", ", groupsOfUser) + "}; Authorized groups: {" + string.Join(", ", this._StaticGroupUserAuthorizationServiceConfiguration.AuthorizedGroups[action]) + "}", Microsoft.Extensions.Logging.LogLevel.Trace);
            return result;
        }
    }
}
