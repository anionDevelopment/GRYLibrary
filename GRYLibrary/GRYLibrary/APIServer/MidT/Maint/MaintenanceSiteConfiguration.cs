using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.MidT.Maint
{
    public class MaintenanceSiteConfiguration : IMaintenanceSiteConfiguration
    {
        public bool Enabled { get; set; } = true;
        public bool MaintenanceModeEnabled { get; set; }

        public virtual ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>();
        }
    }
}
