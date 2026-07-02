namespace GRYLibrary.Core.APIServer.MidT.Maint
{
    public interface ISupportMaintenanceSiteMiddleware : ISupportedMiddleware
    {
        IMaintenanceSiteConfiguration ConfigurationForMaintenanceSiteMiddleware { get; }
    }
}
