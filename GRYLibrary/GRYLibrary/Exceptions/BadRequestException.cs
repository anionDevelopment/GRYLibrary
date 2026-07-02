using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Exceptions
{
    /// <summary>
    /// Represents a exception which is supposed to result in a 4xx-response-statuscode if the exception is thrown in the context of processing a HTTP-request.
    /// </summary>
    public class BadRequestException : UserFormattedException
    {
        public ushort HTTPStatusCode { get; }
        public BadRequestException() : this(StatusCodes.Status400BadRequest, GetMessage(StatusCodes.Status400BadRequest))
        {
        }
        public BadRequestException(string message) : this(StatusCodes.Status400BadRequest, message)
        {
        }
        public BadRequestException(ushort httpStatusCode) : this(httpStatusCode, GetMessage(httpStatusCode))
        {
        }
        public BadRequestException(ushort httpStatusCode, string message) : base(message)
        {
            GUtilities.AssertCondition((httpStatusCode / 100) == 4);
            this.HTTPStatusCode = httpStatusCode;
        }
        public BadRequestException(ushort httpStatusCode, bool verbose, SimpleRequest request) : this(httpStatusCode, AdaptMessage("Request resulted in a bad-request-response", request, verbose))
        {
        }
        public BadRequestException(ushort httpStatusCode, string message, bool verbose, SimpleRequest request) : this(httpStatusCode, AdaptMessage(message, request, verbose))
        {
        }

        private static string AdaptMessage(string message, SimpleRequest request, bool verbose)
        {
            string bodyLog;
            if (verbose)
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                (string bodyInfo, string bodyContent, byte[] bodyPlainContent) = DRequestLoggingMiddleware.BytesToString(request.Body, new UTF8Encoding(false));
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                bodyLog = $" ;Body ({bodyInfo}): {bodyContent}";
            }
            else
            {
                bodyLog = string.Empty;
            }
            string query;
            if (string.IsNullOrEmpty(request.Query))
            {
                query = string.Empty;
            }
            else
            {
                query = "?" + request.Query;
            }
            return $"{message}; Request: {request.HTTPMethod} {request.Route}{query}{bodyLog}";
        }

        private static string GetMessage(ushort httpStatusCode)
        {
            if (httpStatusCode == StatusCodes.Status401Unauthorized)
            {
                return "Authentication required. Please login to authenticate.";
            }
            else if (httpStatusCode == StatusCodes.Status403Forbidden)
            {
                return "Unauthorized";
            }
            else
            {
                return "Bad request";
            }
        }
    }
}