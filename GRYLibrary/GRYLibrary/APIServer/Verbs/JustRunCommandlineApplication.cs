using CommandLine;
using GRYLibrary.Core.Misc.ConsoleApplication;

namespace GRYLibrary.Core.APIServer.Verbs
{
    [Verb(nameof(RunServer), isDefault: true, HelpText = "Runs the server.")]
    public abstract class JustRunCommandlineApplication : ICommandlineParameter
    {
        [Option(nameof(TestRun), Required = false, Default = false)]
        public bool TestRun { get; set; }
    }
}
