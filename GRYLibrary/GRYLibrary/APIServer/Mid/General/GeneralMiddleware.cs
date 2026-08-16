using GRYLibrary.Core.APIServer.MidT.General;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.Mid.General
{
    public class GeneralMiddleware<PersistedApplicationSpecificConfiguration> : GeneralMiddlewareT
        where PersistedApplicationSpecificConfiguration : new()
    {
        private readonly IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> _PersistedAPIServerConfiguration;
        public GeneralMiddleware(RequestDelegate next, IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedAPIServerConfiguration) : base(next)
        {
            this._PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
        }

        public override Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-RequestId") && !string.IsNullOrEmpty(context.Request.Headers["X-RequestId"]))
            {
                context.Items["RequestId"] = context.Request.Headers["X-RequestId"];
            }
            else
            {
                context.Items["RequestId"] = Guid.NewGuid().ToString();
            }
            context.Items["ClientIPAddress"] = this.GetIPAddress(context);
            return this._Next(context);
        }

        protected IPAddress? GetIPAddress(HttpContext context)
        {
            IPAddress? result = context.Connection.RemoteIpAddress;
            if (this._PersistedAPIServerConfiguration.ServerConfiguration.TrustForwardedHeader)
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value) && (value != default(string)))
                {
                    result = IPAddress.Parse((string)value!);
                }
            }
            if (result != null)
            {
                if (result.IsIPv4MappedToIPv6)
                {
                    result = result.MapToIPv4();
                }
            }
            return result;
        }
    }
}
