using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT
{
    /// <summary>
    /// Represents a base-class for middlewares in the context of .NET WebAPI-projects.
    /// </summary>
    public abstract class AbstractMiddleware
    {
        protected readonly RequestDelegate _Next;
        /// <summary>
        /// Creates a new middleware.
        /// </summary>
        public AbstractMiddleware(RequestDelegate next)
        {
            this._Next = next;
        }
        public abstract Task Invoke(HttpContext context);
        public bool EndPointAvailable(HttpContext context)
        {
            return context.GetEndpoint() != null;
        }

        public bool TryGetAuthenticateAttribute(HttpContext context, out AuthenticateAttribute authenticateAttribute)
        {
            return this.TryGetAttributeFromContext(context, out authenticateAttribute);
        }

        public bool TryGetAuthorizeAttribute(HttpContext context, out AuthorizeAttribute authorizedAttribute)
        {
            return this.TryGetAttributeFromContext(context, out authorizedAttribute);
        }

        public bool TryGetActionAttribute(HttpContext context, out ActionAttribute actionAttribute)
        {
            return this.TryGetAttributeFromContext(context, out actionAttribute);
        }

        private bool TryGetAttributeFromContext<T>(HttpContext context, out T attribute)
            where T : Attribute
        {
            try
            {
                Endpoint endPoint = context.GetEndpoint() ?? throw new BadRequestException((int)System.Net.HttpStatusCode.NotFound, "Not found");
                EndpointMetadataCollection metaData = endPoint?.Metadata;
                ControllerActionDescriptor controllerActionDescriptor = metaData?.GetMetadata<ControllerActionDescriptor>();
                System.Reflection.MethodInfo methodInfo = controllerActionDescriptor?.MethodInfo;
                attribute = methodInfo?.GetCustomAttributes(false).OfType<T>().FirstOrDefault();
                return attribute != null;
            }
            catch
            {
                attribute = null;
                return false;
            }
        }
    }
}