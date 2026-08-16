using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace GRYLibrary.Core.APIServer.Settings
{
    /// <summary>
    /// Represents a container for technical information which are required to start a WebAPI-server.
    /// </summary>
    public class FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
        where PersistedApplicationSpecificConfiguration : new()
    {
        public FunctionalInformationForWebApplication(
            InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> initializationInformation,
            IServiceCollection serviceCollection,
            IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedAPIServerConfiguration,
            WebApplication webApplication)
        {
            this.InitializationInformation = initializationInformation;
            this.ServiceCollection = serviceCollection;
            this.PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
            this.WebApplication = webApplication;
        }
        public InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> InitializationInformation { get; internal set; }
        public IServiceCollection ServiceCollection { get; internal set; }
        public IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> PersistedAPIServerConfiguration { get; internal set; }
        public WebApplication WebApplication { get; internal set; }
        public bool RunAsync { get; set; } = false;
        public Action PreRun { get; set; } = () => { };
        /// <remarks>
        /// This will can not be executed if RunAsync is true because the run-method immediately returns a return-code and with this the execution-overhead will be finished..
        /// </remarks>
        public Action PostRun { get; set; } = () => { };
    }
}
