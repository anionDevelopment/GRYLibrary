namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    public abstract class AbstractHostMaintenanceInformation
    {
        public abstract void Accept(IMaintenanceRoutesHostInformationVisitor visitor);
        public abstract T Accept<T>(IMaintenanceRoutesHostInformationVisitor<T> visitor);
    }
    public interface IMaintenanceRoutesHostInformationVisitor
    {
        void Handle(DoNotHostMaintenanceRoutes doNotHostMaintenanceRoutes);
        void Handle(HostMaintenanceRoutes hostMaintenanceRoutes);
    }
    public interface IMaintenanceRoutesHostInformationVisitor<T>
    {
        T Handle(DoNotHostMaintenanceRoutes doNotHostMaintenanceRoutes);
        T Handle(HostMaintenanceRoutes hostMaintenanceRoutes);
    }
}
