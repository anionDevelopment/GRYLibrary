using CommandLine;

namespace GRYLibrary.Core.Misc.ConsoleApplication
{
    [Verb(nameof(Info), isDefault: false, HelpText = "Shows information")]
    public class Info : InternalCommand
    {
        public override int Run(GRYConsoleApplicationInitialInformation applicationInitialInformation)
        {
            this.ParserBase._Logger.Log($"Version: {applicationInitialInformation.ProgramVersion}", Microsoft.Extensions.Logging.LogLevel.Information);
            if (applicationInitialInformation.ProgramDescription is not null)
            {
                this.ParserBase._Logger.Log($"Description: {applicationInitialInformation.ProgramDescription}", Microsoft.Extensions.Logging.LogLevel.Information);
            }
            return 0;
        }
    }
}
