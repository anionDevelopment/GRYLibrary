using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace GRYLibrary.Core.APIServer.Mid.Ex
{
    public class DefaultExceptionHandlerMiddleware : ExceptionManagerMiddleware
    {
        private readonly IGRYLog _Log;
        public DefaultExceptionHandlerMiddleware(RequestDelegate next, IServerLog logger) : base(next)
        {
            this._Log = logger.Logger;
        }

        protected override void HandleException(HttpContext context, Exception exception)
        {
            Exception exceptionForFormatting;
            if (exception == null)
            {
                throw new NotImplementedException();
            }
            else if (exception is AggregateException aggregateException)
            {
                if (aggregateException.InnerExceptions == null)
                {
                    exceptionForFormatting = new InternalAlgorithmException("No inner exception given.");
                }
                else
                {
                    if (aggregateException.InnerExceptions.Count == 0)
                    {
                        exceptionForFormatting = new InternalAlgorithmException("No inner exceptions given.");
                    }
                    else if (aggregateException.InnerExceptions.Count == 1)
                    {
                        exceptionForFormatting = aggregateException.InnerExceptions[0];
                    }
                    else
                    {
                        exceptionForFormatting = aggregateException;
                    }
                }
            }
            else
            {
                exceptionForFormatting = exception;
            }

            if (exceptionForFormatting is BadRequestException badHttpRequestException)
            {
                context.Response.StatusCode = badHttpRequestException.HTTPStatusCode;
            }
            else if (exceptionForFormatting is InvalidCredentialsException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else if (exceptionForFormatting is NotAuthorizedException)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else if (exceptionForFormatting is NotFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            else if (exceptionForFormatting is InternalAlgorithmException)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                this.Log("Internal error", context, exceptionForFormatting, (uint)context.Response.StatusCode, Microsoft.Extensions.Logging.LogLevel.Error);
            }
            else if (exceptionForFormatting is DependencyNotAvailableException)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                this.Log("Error while processing request", context, exceptionForFormatting, (uint)context.Response.StatusCode, Microsoft.Extensions.Logging.LogLevel.Error);
            }
            (string ContentType, string bodyContent) = this.GetExceptionResponceContent(context.Response.StatusCode, context, exceptionForFormatting);
            context.Response.ContentType = ContentType;
            context.Response.WriteAsync(bodyContent).Wait();
        }

        private void Log(string technicalReason, HttpContext context, Exception exception, uint statuscode,LogLevel loglevel)
        {
            this._Log.Log($"Request {context.Items["RequestId"]} resulted in statuscode {statuscode}. Technical reason: {technicalReason}", exception, loglevel);
        }

        public virtual (string ContentType, string bodyContent) GetExceptionResponceContent(int httpStatusCode, HttpContext context, Exception exception)
        {
            return (null, string.Empty);
        }

        public virtual (string ContentType, string bodyContent) GetNotFoundResponseContent(HttpContext context)
        {
            return (null, string.Empty);
        }
    }
}
