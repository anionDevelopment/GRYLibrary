using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace GRYLibrary.Core.APIServer.Services.CredH
{
    public class HeaderService : IHeaderService
    {
        public IHeaderServiceConfiguration HeaderServiceConfiguration { get; set; }
        public HeaderService(IHeaderServiceConfiguration HeaderServiceConfiguration)
        {
            this.HeaderServiceConfiguration = HeaderServiceConfiguration;
        }
        public virtual string ExtractSecret(HttpContext context)
        {
            this.TryGetHeaderValue(context, out string result);
            return result;
        }

        public virtual bool ContainsCredentials(HttpContext context)
        {
            return this.TryGetHeaderValue(context, out string _);
        }

        public virtual bool TryGetHeaderValue(HttpContext context, out string header)
        {
            bool result = context.Request.Headers.TryGetValue(HeaderTools.HeaderName, out StringValues headerAsStrings);
            header = headerAsStrings!;
            return result && headerAsStrings != default(StringValues);
        }

        public (string key, string value) CreateHeader(string username, string value, DateTime expiredMoment)
        {
            return HeaderTools.GetAccessTokenHeader(username, value, expiredMoment);
        }

        public (string key, string value) GetAccessTokenExpiredHeader(string name)
        {
            return HeaderTools.GetAccessTokenExpiredHeader(name);
        }
    }
}
