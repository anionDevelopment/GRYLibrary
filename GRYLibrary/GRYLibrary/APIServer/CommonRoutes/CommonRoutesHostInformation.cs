namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    public abstract class CommonRoutesHostInformation
    {
        public abstract void Accept(ICommonRoutesHostInformationVisitor visitor);
        public abstract T Accept<T>(ICommonRoutesHostInformationVisitor<T> visitor);
    }
    public interface ICommonRoutesHostInformationVisitor
    {
        void Handle(DoNotHostCommonRoutes doNotHostCommonRoutes);
        void Handle(HostCommonRoutes hostCommonRoutes);
    }
    public interface ICommonRoutesHostInformationVisitor<T>
    {
        T Handle(DoNotHostCommonRoutes doNotHostCommonRoutes);
        T Handle(HostCommonRoutes hostCommonRoutes);
    }
}
