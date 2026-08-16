using CommandLine;

namespace GRYLibrary.Core.APIServer.Verbs
{
    [Verb(nameof(RunServer), isDefault: true, HelpText = "Runs the server.")]
    public abstract class RunServer : IAPIServerCommandlineParameter
    {
        [Option(nameof(RealRun), Required = false, Default = false)]
        public bool RealRun { get; set; }

        [Option(nameof(InitialVerboseValue), Required = false, Default = true)]
        public bool InitialVerboseValue { get; set; }

        [Option(nameof(EnforceVerbose), Required = false, Default = false)]
        public bool EnforceVerbose { get; set; }
    }
}
