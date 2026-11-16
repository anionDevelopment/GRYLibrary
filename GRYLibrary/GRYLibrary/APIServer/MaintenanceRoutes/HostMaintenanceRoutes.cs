using System;

namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    public class HostMaintenanceRoutes : AbstractHostMaintenanceInformation
    {
        public Type ControllerType { get; set; } = typeof(MaintenanceRoutesController);
        /// <summary>
        /// Path which is prepended to the routes of the maintenance-routes-controller.
        /// For example with the value "/MyApp" the health-check is available on "/MyApp/API/Other/Maintenance/HealthCheck".
        /// An empty value keeps the routes unchanged.
        /// </summary>
        public string RoutePrefix { get; set; } = string.Empty;
        #region Overhead
        public override void Accept(IMaintenanceRoutesHostInformationVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IMaintenanceRoutesHostInformationVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
        #endregion
    }
}
