using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Verbs;

namespace GRYLibrary.Core.APIServer.Services.Init
{
    public interface IInitializationService
    {
        public InitializationState GetInitializationState();
    }
    /// <summary>
    /// Does business-logic-related initialization.
    /// </summary>
    public interface IInitializationService<CommandlineParameter> : IInitializationService
        where CommandlineParameter : RunServer
    {
        public void Initialize(CommandlineParameter commandlineParameter);
    }
}
