using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredC
{
    public static class CookieTools
    {
        public static string CookieName { get; set; } = "X-Authorization";
        public static (string key, string value, CookieOptions options) GetAccessTokenCookie(string username, string accessToken, DateTimeOffset expires)
        {
            return GetCookieWithSpecificExpiredDate(username, expires, accessToken);
        }

        public static (string key, string value, CookieOptions options) GetAccessTokenExpiredCookie(string username)
        {
            return GetCookieWithSpecificExpiredDate(username, new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero), string.Empty);
        }

        public static (string key, string value, CookieOptions options) GetCookieWithSpecificExpiredDate(string username, DateTimeOffset expiredDate, string accessToken)
        {
            return (CookieName,
                $"User={username};AccessToken={accessToken}",
                new CookieOptions()
                {
                    Expires = expiredDate,
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    // SameSite=Strict is the CSRF mitigation for the authentication cookie: the
                    // browser will not attach it to any cross-site request, so a forged POST from a
                    // third-party page cannot ride the victim's session. These are server-rendered
                    // apps whose auth cookie never legitimately needs to travel on a cross-site
                    // top-level navigation, so Strict has no functional downside here.
                    SameSite = SameSiteMode.Strict,
                }
            );
        }
    }
}
