namespace GRYLibrary.Core.APIServer.MaintenanceRoutes
{
    public class DoNotHostMaintenanceRoutes : AbstractHostMaintenanceInformation
    {
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
