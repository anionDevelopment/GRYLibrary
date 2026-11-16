using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    /// <summary>
    /// Prepends <see cref="HostMaintenanceRoutes.RoutePrefix"/> to the routes of the <see cref="MaintenanceRoutesController"/>.
    /// </summary>
    /// <remarks>
    /// This allows applications which forward all other requests (for example reverse-proxies) to host their maintenance-routes
    /// under a path which does not collide with the routes of the server they forward to.
    /// </remarks>
    public sealed class MaintenanceRoutesPrefixConvention : IControllerModelConvention
    {
        private readonly string _RoutePrefix;

        public MaintenanceRoutesPrefixConvention(string routePrefix)
        {
            this._RoutePrefix = routePrefix.Trim('/');
        }

        public void Apply(ControllerModel controller)
        {
            if (!typeof(MaintenanceRoutesController).IsAssignableFrom(controller.ControllerType))
            {
                return;
            }
            if (this._RoutePrefix.Length == 0)
            {
                return;
            }
            foreach (SelectorModel selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel.Template = $"/{this._RoutePrefix}/{selector.AttributeRouteModel.Template!.TrimStart('/')}";
                }
            }
        }
    }
}
