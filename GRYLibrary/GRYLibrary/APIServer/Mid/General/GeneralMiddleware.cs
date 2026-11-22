using GRYLibrary.Core.APIServer.MidT.General;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
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
                    result = GetClientIPAddressFromForwardedForHeader(value!);
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

        /// <summary>
        /// Returns the client-ip-address from the value of an "X-Forwarded-For"-header.
        /// </summary>
        /// <remarks>
        /// The header can contain several comma-separated addresses (and can also be sent several times), where every proxy appends the address of its client.
        /// Only the last address is taken because it is the one which was appended by the trusted reverse-proxy directly in front of this server.
        /// All addresses before it can be set arbitrarily by the client, so taking one of them would allow to spoof the client-ip-address.
        /// </remarks>
        /// <exception cref="FormatException">Thrown if the last address is not a valid ip-address.</exception>
        internal static IPAddress GetClientIPAddressFromForwardedForHeader(Microsoft.Extensions.Primitives.StringValues headerValue)
        {
            string lastAddress = string.Join(",", headerValue.ToArray()).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
            return IPAddress.Parse(lastAddress);
        }
    }
}
