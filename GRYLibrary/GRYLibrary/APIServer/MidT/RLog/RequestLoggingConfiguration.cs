using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.MidT.RLog
{
    /// <summary>
    /// Configuration for <see cref="RequestLoggingMiddleware"/>
    /// </summary>
    public class RequestLoggingConfiguration : IRequestLoggingConfiguration
    {
        public bool Enabled { get; set; } = true;

        public virtual ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>();
        }
    }
}