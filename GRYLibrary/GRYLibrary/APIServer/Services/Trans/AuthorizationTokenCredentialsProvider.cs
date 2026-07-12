using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.Trans
{
    public class AuthorizationTokenCredentialsProvider : IHTTPCredentialsProvider
    {
        public bool ContainsCredentials(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public string ExtractSecret(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
