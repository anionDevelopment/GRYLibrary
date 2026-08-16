using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredH
{
    public interface IHeaderService : ICredentialsProvider
    {
        public abstract bool TryGetHeaderValue(HttpContext context, out string Header);
        (string key, string value) CreateHeader(string username, string value, DateTime expiredMoment);
        (string key, string value) GetAccessTokenExpiredHeader(string name);
    }
}
