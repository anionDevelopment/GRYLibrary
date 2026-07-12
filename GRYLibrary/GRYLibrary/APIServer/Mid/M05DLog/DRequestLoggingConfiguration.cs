using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.FilePath;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Mid.M05DLog
{
    public class DRequestLoggingConfiguration : IDRequestLoggingConfiguration
    {
        public bool AddMillisecondsInLogTimestamps { get; set; } = false;
        public GRYLogConfiguration RequestsLogConfiguration { get; set; } = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./Requests.log"), true);
        public bool Enabled { get; set; } = true;
        public bool LogClientIP { get; set; } = true;
        public uint MaximalLengthofRequestBodies { get; set; } = 4000;
        public uint MaximalLengthOfResponseBodies { get; set; } = 4000;
        public ISet<string> NotLoggedRoutes { get; set; } = new HashSet<string>();
        public ISet<string> LoggedHTTPRequeustHeader { get; set; } = new HashSet<string>();


        public ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>();
        }
    }
}