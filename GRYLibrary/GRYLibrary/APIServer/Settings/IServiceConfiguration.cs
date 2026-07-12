using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.APIServer.Settings
{
    public interface IServiceConfiguration
    {
        public bool Enabled { get; set; }
        public GRYLogConfiguration LogConfiguration { get; set; }
    }
}
