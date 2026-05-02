using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.Maint
{
    public abstract class MaintenanceSiteMiddleware : AbstractMiddleware
    {
        private readonly IMaintenanceSiteConfiguration _MaintenanceSiteConfiguration;
        private readonly IInitializationService _InitializationService;
        protected abstract (string ContentType, string bodyContent) GetMaintenanceSite(HttpContext context);
        //Returns true if and only if the context is allowed to be loaded even in maintenance-mode.
        protected MaintenanceSiteMiddleware(RequestDelegate next, IInitializationService initializationService, IMaintenanceSiteConfiguration maintenanceSiteConfiguration) : base(next)
        {
            this._MaintenanceSiteConfiguration = maintenanceSiteConfiguration;
            this._InitializationService = initializationService;
        }
        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            if (this._MaintenanceSiteConfiguration.MaintenanceModeEnabled || this._InitializationService.GetInitializationState() is not Initialized)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                (string ContentType, string bodyContent) = this.GetMaintenanceSite(context);
                context.Response.ContentType = ContentType;
                context.Response.WriteAsync(bodyContent).Wait();
            }
            else
            {
                this._Next(context).Wait();
            }
            return Task.CompletedTask;
        }
        protected virtual bool IsException(HttpContext context)
        {
            return false;//TODO implement and use this function properly
        }
    }
}