namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    /// <summary>
    /// Represents the configuration for <see cref="MaintenanceRoutesController"/>.
    /// </summary>
    public interface IMaintenanceRoutesInformation
    {
        public bool EnableEndpointAvailabilityCheck { get; set; }
        public bool EnableEndpointInitializationState { get; set; }
        public bool EnableEndpointCurrentVersion { get; set; }
        public bool EnableEndpointShowAllEndpoints { get; set; }
        public bool EnableEndpointHealthCheck { get; set; }
        public bool EnableEndpointMetrics { get; set; }
    }
}
