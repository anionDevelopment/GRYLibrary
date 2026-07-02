using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.Aut
{
    public abstract class AuthorizationMiddleware : AbstractMiddleware
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IAuthorizationConfiguration _AuthorizationConfiguration;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly IGRYLog _Log;
        protected AuthorizationMiddleware(RequestDelegate next, IGRYLog log, IAuthorizationConfiguration authorizationConfiguration) : base(next)
        {
            this._AuthorizationConfiguration = authorizationConfiguration;
            this._Log = log;
        }
        protected abstract bool IsAuthorized(HttpContext context);
        public override Task Invoke(HttpContext context)
        {
            if (this.AuthorizationIsRequired(context) && !this.IsAuthorizedInternal(context))
            {
                throw new BadRequestException(StatusCodes.Status403Forbidden);
            }
            else
            {
                return this._Next(context);
            }
        }
        public bool AuthorizationIsRequired(HttpContext context)
        {
            if (this.TryGetAuthorizeAttribute(context, out AuthorizeAttribute authorizeAttribute))
            {
                bool result = authorizeAttribute.Groups.Any();
                if (result)
                {
                    this._Log.Log($"Authorization for request {context.Items["RequestId"]} is required because of authorized-groups: {string.Join(", ", authorizeAttribute.Groups)}", Microsoft.Extensions.Logging.LogLevel.Trace);
                }
                else
                {
                    this._Log.Log($"Authorization for request {context.Items["RequestId"]} is not required because of empty authorized-groups-set.", Microsoft.Extensions.Logging.LogLevel.Trace);
                }
                return result;
            }
            else
            {
                this._Log.Log($"Authorization for request {context.Items["RequestId"]} is not required.", Microsoft.Extensions.Logging.LogLevel.Trace);
                return false;
            }
        }

        public virtual bool IsAuthorizedInternal(HttpContext context)
        {
            if (this.IsAuthorized(context))
            {
                this._Log.Log($"User of request {context.Items["RequestId"]} is authorized.", Microsoft.Extensions.Logging.LogLevel.Trace);
                return true;
            }
            else
            {
                this._Log.Log($"User of request {context.Items["RequestId"]} is not authorized.", Microsoft.Extensions.Logging.LogLevel.Trace);
                return false;
            }
        }
    }
}
