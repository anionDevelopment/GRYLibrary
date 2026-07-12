using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.Exception
{
    /// <summary>
    /// Represents a middleware which handles exceptions.
    /// </summary>
    public abstract class ExceptionManagerMiddleware : AbstractMiddleware
    {
        /// <inheritdoc/>
        public ExceptionManagerMiddleware(RequestDelegate next) : base(next)
        {
        }
        protected abstract void HandleException(HttpContext context, System.Exception exception);
        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            try
            {
                this._Next(context).Wait();
            }
            catch (System.Exception exception)
            {
                try
                {
                    this.HandleException(context, exception);
                }
                catch (System.Exception e)
                {
                    System.Console.Error.WriteLine("Error while handling error-response: " + e.ToString());
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }
            return Task.CompletedTask;
        }
    }
}