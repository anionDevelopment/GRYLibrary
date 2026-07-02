using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.APIK
{
    public class APIKeyAuthorizationService : IAPIKeyAuthorizationService
    {
        private readonly Func<string/*action*/, string/*apiKey*/, bool> _Validator;
        public APIKeyAuthorizationService(Func<string/*action*/, string/*apiKey*/, bool> validator)
        {
            this._Validator = validator;
        }

        public bool IsAuthorized(string action, string apiKey)
        {
            return this._Validator(action, apiKey);
        }

        public bool IsAuthorized(HttpContext context, AuthorizeAttribute authorizedAttribute)
        {
            throw new NotImplementedException();
        }
    }
}