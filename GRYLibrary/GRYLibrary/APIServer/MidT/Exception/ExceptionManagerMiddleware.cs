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

        /// <summary>
        /// Fills the body of a response which says "not found" but has no content.
        ///
        /// This is required because a request whose address belongs to no route of the application does not
        /// result in an exception: the pipeline simply answers with the status-code 404 and an empty body. Without
        /// this the user would get an empty page for every address which does not exist.
        ///
        /// It does nothing by default, so an application which does not want an own page for this keeps the
        /// behaviour it had.
        /// </summary>
        protected virtual void HandleNotFound(HttpContext context)
        {
        }

        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            try
            {
                this._Next(context).Wait();
                this.HandleNotFoundIfRequired(context);
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

        private void HandleNotFoundIfRequired(HttpContext context)
        {
            if (context.Response.StatusCode != StatusCodes.Status404NotFound)
            {
                return;
            }
            // A response which was already sent can not be extended anymore. This is the regular case for a
            // route which answered with 404 itself and wrote its own body.
            if (context.Response.HasStarted)
            {
                return;
            }
            this.HandleNotFound(context);
        }
    }
}