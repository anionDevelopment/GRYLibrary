using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredC
{
    public static class CookieTools
    {
        public static string CookieName { get; set; } = "X-Authorization";
        public static (string key, string value, CookieOptions options) GetAccessTokenCookie(HttpContext context, string username, string accessToken, DateTimeOffset expires)
        {
            return GetCookieWithSpecificExpiredDate(context, username, expires, accessToken);
        }

        public static (string key, string value, CookieOptions options) GetAccessTokenExpiredCookie(HttpContext context, string username)
        {
            return GetCookieWithSpecificExpiredDate(context, username, new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero), string.Empty);
        }

        public static (string key, string value, CookieOptions options) GetCookieWithSpecificExpiredDate(HttpContext context, string username, DateTimeOffset expiredDate, string accessToken)
        {
            return (CookieName,
                $"User={username};AccessToken={accessToken}",
                new CookieOptions()
                {
                    Expires = expiredDate,
                    Path = "/",
                    HttpOnly = true,
                    // Secure is bound to the protocol of the current request (the "same-as-request"-policy):
                    // over HTTPS the cookie must never travel unencrypted, while over plain HTTP a secure
                    // cookie would never be sent back by the client at all, which would break the session.
                    Secure = context.Request.IsHttps,
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
