using System;

namespace GRYLibrary.Core.APIServer.Utilities
{
    /// <summary>
    /// Prevents that the request-body of the annotated controller-method is written to the request-log.
    /// </summary>
    /// <remarks>
    /// This takes precedence over the configuration of the request-logging-middleware, so the request-body is not logged regardless of how the request-logging is configured.
    /// Use this for example for login-, token- or password-related routes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class PreventRequestBodyLogAttribute : Attribute
    {
    }
}
