using System;
using System.Threading;

namespace GRYLibrary.Core.APIServer.Settings
{
    public class APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
        where PersistedApplicationSpecificConfiguration : new()
    {
        public APIServerConfiguration()
        {

        }
        internal InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> InitializationInformation { get; set; }
        public Action<InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>> SetInitialzationInformationAction { get; set; } = (_) => { };
        internal FunctionalInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> FunctionalInformation { get; set; }
        public Action<FunctionalInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>> SetFunctionalInformationAction { get; set; } = (_) => { };
        internal FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> FunctionalInformationForWebApplication { get; set; }
        public Action<FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>> ConfigureWebApplication { get; set; } = (_) => { };
        public CommandlineParameterType CommandlineParameter { get; set; }
        /// <summary>
        /// Token to stop the web-API.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
