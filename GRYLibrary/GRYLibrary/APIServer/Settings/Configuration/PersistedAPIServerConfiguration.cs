using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    /// <summary>
    /// Represents Application-constants which are editable by a configuration-file.
    /// </summary>
    public interface IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration>
        where PersistedApplicationSpecificConfiguration : new()
    {
        public PersistedApplicationSpecificConfiguration ApplicationSpecificConfiguration { get; set; }
        public IServerConfiguration ServerConfiguration { get; set; }
        public GRYLogConfiguration ApplicationLogConfiguration { get; set; }
    }

    public class PersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> : IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration>
        where PersistedApplicationSpecificConfiguration : new()
    {
        public IServerConfiguration ServerConfiguration { get; set; }
        public GRYLogConfiguration ApplicationLogConfiguration { get; set; }
        public PersistedApplicationSpecificConfiguration ApplicationSpecificConfiguration { get; set; }
        public static PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> Create<PersistedAppSpecificConfiguration>(PersistedAppSpecificConfiguration persistedApplicationSpecificConfiguration, GRYEnvironment environment)
            where PersistedAppSpecificConfiguration : new()
        {
            ServerConfiguration serverConfiguration = new ServerConfiguration();
            PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> result = new PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>
            {
                ServerConfiguration = serverConfiguration,
                ApplicationLogConfiguration = GRYLogConfiguration.GetCommonConfiguration("Server.log", environment is Development),
                ApplicationSpecificConfiguration = persistedApplicationSpecificConfiguration
            };
            return result;
        }
    }
}
