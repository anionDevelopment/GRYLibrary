using System;

namespace GRYLibrary.Core.APIServer.Services.CredH
{
    public static class HeaderTools
    {
        public static string HeaderName { get; set; } = "X-AccessToken";
        public static (string key, string value) GetAccessTokenHeader(string username, string accessToken, DateTime expires)
        {
            return GetHeaderWithSpecificExpiredDate(username, expires, accessToken);
        }

        public static (string key, string value) GetAccessTokenExpiredHeader(string username)
        {
            return GetHeaderWithSpecificExpiredDate(username, new DateTime(1970, 1, 1, 0, 0, 0), string.Empty);
        }

        public static (string key, string value) GetHeaderWithSpecificExpiredDate(string username, DateTime expiredDate, string accessToken)
        {
            return (HeaderName, $"User={username};AccessToken={accessToken}");
        }
    }
}
