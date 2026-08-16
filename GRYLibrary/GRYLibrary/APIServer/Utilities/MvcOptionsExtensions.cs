using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GRYLibrary.Core.APIServer.Utilities
{

    public static class MvcOptionsExtensions
    {
        public static void UseGeneralRoutePrefix(this MvcOptions mvcOptions, IRouteTemplateProvider routeAttribute)
        {
            mvcOptions.Conventions.Add(new RoutePrefixConvention(routeAttribute));
        }

        public static void UseGeneralRoutePrefix(this MvcOptions mvcOptions, string prefix)
        {
            mvcOptions.UseGeneralRoutePrefix(new RouteAttribute(prefix));
        }
    }
}