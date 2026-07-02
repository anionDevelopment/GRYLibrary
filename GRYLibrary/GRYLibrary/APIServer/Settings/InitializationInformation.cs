using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.FilePath;

namespace GRYLibrary.Core.APIServer.Settings
{
    public class InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
        where PersistedApplicationSpecificConfiguration : new()
    {
        public InitializationInformation()
        {
        }
        public CommandlineParameterType CommandlineParameter { get; internal set; }
        public string BaseFolder { get; set; }
        public IApplicationConstants<ApplicationSpecificConstants> ApplicationConstants { get; internal set; }
        /// <summary>
        /// Represents the default-value for the configuration which should be used when there is not already a persisted configuration which can be loaded.
        /// </summary>
        public PersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> InitialApplicationConfiguration { get; set; }
        public AbstractFilePath BasicInformationFile { get; internal set; }
        public IGRYLog InitialLogger { get; set; }
    }
}
