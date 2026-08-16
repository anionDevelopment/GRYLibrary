using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredC
{
    public class CookieService : ICookieService
    {
        public ICookieServiceConfiguration CookieServiceConfiguration { get; set; }
        public CookieService(ICookieServiceConfiguration cookieServiceConfiguration)
        {
            this.CookieServiceConfiguration = cookieServiceConfiguration;
        }
        public virtual string ExtractSecret(HttpContext context)
        {
            this.TryGetCookieValue(context, out string result);
            return result;
        }

        public virtual bool ContainsCredentials(HttpContext context)
        {
            return this.TryGetCookieValue(context, out string _);
        }

        public virtual bool TryGetCookieValue(HttpContext context, out string cookie)
        {
            return context.Request.Cookies.TryGetValue(CookieTools.CookieName, out cookie);
        }

        public (string key, string value, CookieOptions options) CreateCookie(HttpContext context, string username, string value, DateTimeOffset expiredMoment)
        {
            return CookieTools.GetAccessTokenCookie(context, username, value, expiredMoment);
        }

        public (string key, string value, CookieOptions options) GetAccessTokenExpiredCookie(HttpContext context, string name)
        {
            return CookieTools.GetAccessTokenExpiredCookie(context, name);
        }
    }
}
