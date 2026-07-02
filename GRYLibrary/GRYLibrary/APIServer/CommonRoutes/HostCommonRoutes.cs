using System;

namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    public class HostCommonRoutes : CommonRoutesHostInformation
    {
        public Type ControllerType { get; set; } = typeof(CommonRoutesController);
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
