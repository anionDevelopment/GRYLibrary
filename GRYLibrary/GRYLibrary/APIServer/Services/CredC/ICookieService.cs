using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredC
{
    public interface ICookieService : ICredentialsProvider
    {
        public abstract bool TryGetCookieValue(HttpContext context, out string cookie);
        (string key, string value, CookieOptions options) CreateCookie(HttpContext context, string username, string value, DateTimeOffset expiredMoment);
        (string key, string value, CookieOptions options) GetAccessTokenExpiredCookie(HttpContext context, string name);
    }
}
