namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    public class DoNotHostCommonRoutes : CommonRoutesHostInformation
    {
        #region Overhead
        public override void Accept(ICommonRoutesHostInformationVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(ICommonRoutesHostInformationVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
        #endregion
    }
}
