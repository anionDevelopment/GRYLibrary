namespace GRYLibrary.Core.APIServer.MidT.Maint
{
    public interface IMaintenanceSiteConfiguration : IMiddlewareConfiguration
    {
        public bool MaintenanceModeEnabled { get; set; }
    }
}
