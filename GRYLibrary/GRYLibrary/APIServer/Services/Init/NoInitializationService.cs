using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.APIServer.Verbs;

namespace GRYLibrary.Core.APIServer.Services.Init
{
    public class NoInitializationService<CmdArguments> : IInitializationService<CmdArguments>
        where CmdArguments : RunServer
    {
        public InitializationState GetInitializationState()
        {
            return new Initialized();
        }

        public void Initialize(CmdArguments commandlineParameter)
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
    }
}
