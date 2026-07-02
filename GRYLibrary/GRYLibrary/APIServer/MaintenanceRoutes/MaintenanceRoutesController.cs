using Microsoft.AspNetCore.Mvc;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GRYLibrary.Core.Misc;
using System.IO;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Utilities;

namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    [ApiController]
    [Route($"{ServerConfiguration.APIRoutePrefix}/Other/Maintenance")]
    public class MaintenanceRoutesController : Controller
    {
        private readonly IApplicationConstants _Configuration;
        private readonly IMaintenanceRoutesInformation _MaintenanceRoutesInformation;
        private readonly IEnumerable<EndpointDataSource> _EndpointSources;
        private readonly IHealthCheck _HealthCheck;
        private readonly IInitializationService _InitializationService;
        public MaintenanceRoutesController(IApplicationConstants configuration, IMaintenanceRoutesInformation maintenanceRoutesInformation, IEnumerable<EndpointDataSource> endpointSources, IHealthCheck healthCheck, IInitializationService initializationService)
        {
            this._Configuration = configuration;
            this._MaintenanceRoutesInformation = maintenanceRoutesInformation;
            this._EndpointSources = endpointSources;
            this._HealthCheck = healthCheck;
            this._InitializationService = initializationService;
        }

        private bool EndpointEnabled(bool endpointEnabled, out IActionResult? disabledResult)
        {
            if (endpointEnabled)
            {
                disabledResult = null;
            }
            else
            {
                disabledResult = this.NotFound();
            }
            return endpointEnabled;
        }

        [HttpGet]
        [Route(nameof(AvailabilityCheck))]
        public virtual IActionResult AvailabilityCheck()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointAvailabilityCheck, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            return this.Ok();
        }

        [HttpGet]
        [Route(nameof(InitializationState))]
        public virtual IActionResult InitializationState()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointInitializationState, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            InitializationState result = this._InitializationService.GetInitializationState();
            return this.Ok(result.ToString());
        }

        [HttpGet]
        [Route(nameof(CurrentVersion))]
        public virtual IActionResult CurrentVersion()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointCurrentVersion, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            return this.Ok(Assembly.GetEntryAssembly().GetName().Version.ToString(3));
        }

        [HttpGet(nameof(ShowAllEndpoints))]
        public virtual IActionResult ShowAllEndpoints()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointShowAllEndpoints, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            IEnumerable<RouteEndpoint> endpoints = this._EndpointSources.SelectMany(es => es.Endpoints).OfType<RouteEndpoint>();
            var output = endpoints.Select(endPoint =>
            {
                ControllerActionDescriptor controller = endPoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();
                string action = controller != null ? $"{controller.ControllerName}.{controller.ActionName}" : null;
                string controllerMethod = controller != null ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}" : null;
                return new
                {
                    Method = endPoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                    Route = $"/{endPoint.RoutePattern.RawText.TrimStart('/')}",
                    Action = action,
                    ControllerMethod = controllerMethod
                };
            });
            return this.Json(output);
        }

        /// <returns>
        /// Returns a JSON with a "status"-property.
        /// Status-meaning:
        /// 0 = Unhealthy
        /// 1 = Degraded
        /// 2 = Healthy
        /// </returns>
        /// <example>
        /// {"data":{},"description":"Service is healthy.","exception":null,"status":2}
        ///  </example>
        [HttpGet]
        [Route(nameof(HealthCheck))]
        public virtual IActionResult HealthCheck()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointHealthCheck, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            return this.Ok(this._HealthCheck.CheckHealthAsync(new HealthCheckContext()).WaitAndGetResult());
        }

        [HttpGet]
        [Route(nameof(Metrics))]
        public virtual IActionResult Metrics()
        {
            if (!this.EndpointEnabled(this._MaintenanceRoutesInformation.EnableEndpointMetrics, out IActionResult? disabledResult))
            {
                return disabledResult!;
            }
            try
            {
                using MemoryStream ms = new MemoryStream();
                Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                ms.Position = 0;
                using StreamReader sr = new StreamReader(ms);
                string allmetrics = sr.ReadToEndAsync().WaitAndGetResult();
                return this.Ok(allmetrics);
            }
            catch
            {
                throw;
            }
        }
    }
}
