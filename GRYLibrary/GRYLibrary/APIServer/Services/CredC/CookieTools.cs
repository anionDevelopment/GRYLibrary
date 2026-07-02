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
                }
            );
        }
    }
}
