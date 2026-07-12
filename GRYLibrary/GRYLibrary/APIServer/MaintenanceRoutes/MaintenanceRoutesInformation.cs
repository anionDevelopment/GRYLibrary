namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    public class MaintenanceRoutesInformation : IMaintenanceRoutesInformation
    {
        public bool EnableEndpointAvailabilityCheck { get; set; } = true;
        public bool EnableEndpointInitializationState { get; set; } = true;
        public bool EnableEndpointCurrentVersion { get; set; } = true;
        public bool EnableEndpointShowAllEndpoints { get; set; } = true;
        public bool EnableEndpointHealthCheck { get; set; } = true;
        public bool EnableEndpointMetrics { get; set; } = true;
    }
}
