using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Cerbos
{
    public class CerbosAuthorizationService : IUserAuthorizationService
    {
        private readonly IAuthenticationService _AuthenticationService;
        public CerbosAuthorizationService(IAuthenticationService authenticationService)
        {
            this._AuthenticationService = authenticationService;
        }
        public bool IsAuthorized(string user, string action)
        {
            throw new NotImplementedException();
        }

        public bool IsAuthorized(string user, string action, ISet<string> authorizedGroups)
        {
            throw new NotImplementedException();
        }
    }
}
