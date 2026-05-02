using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using Microsoft.Extensions.Logging;
using System;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using System.IO;
using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.Misc.ConsoleApplication
{
    /// <summary>
    /// Manages overhead for console-programs.
    /// </summary>
    public class GRYConsoleApplication<CMDOptions> where CMDOptions : ICommandlineParameter
    {
        private readonly ParserBase _Mains;
        private readonly string _ProgramName;
        private readonly string _ProgramVersion;
        private readonly string _ProgramDescription;
        private readonly bool _ProgramCanRunWithoutArguments;
        private readonly IGRYLog _Log;
        private readonly ExecutionMode _ExecutionMode;
        private readonly SentenceBuilder _SentenceBuilder;
        private readonly GRYConsoleApplicationInitialInformation _GRYConsoleApplicationInitialInformation;
        public GRYConsoleApplication(ParserBase mains, string programName, string programVersion, string programDescription, bool programCanRunWithoutArguments, ExecutionMode executionMode, GRYEnvironment environment, string? additionalHelpText, IGRYLog log)
        {
            this._Mains = mains;
            this._ProgramName = programName;
            this._ProgramVersion = programVersion;
            this._ProgramDescription = programDescription;
            this._ProgramCanRunWithoutArguments = programCanRunWithoutArguments;
            this._Log = log;
            this._Log.Configuration.PrintEmptyLines = true;
            this._SentenceBuilder = SentenceBuilder.Create();
            this._ExecutionMode = executionMode;
            this._GRYConsoleApplicationInitialInformation = new GRYConsoleApplicationInitialInformation(this._ProgramName, this._ProgramVersion, this._ProgramDescription, this._ExecutionMode, environment, additionalHelpText);
        }

        public int Main(string[] arguments)
        {
            int result = 1;
            try
            {
                string title = $"{this._ProgramName} (v{this._ProgramVersion})";
                Console.Title = title;
                if (arguments == null)
                {
                    throw GUtilities.CreateNullReferenceExceptionDueToParameter(nameof(arguments));
                }
                string argumentsAsString = string.Join(' ', arguments);
                string workingDirectory = Directory.GetCurrentDirectory();
                try
                {
                    this._Log.Log($"Arguments: \"{argumentsAsString}\"", LogLevel.Debug);
                    if (this._ExecutionMode is Analysis)
                    {
                        arguments = Array.Empty<string>();
                    }
                    if (arguments.Length == 0 && !this._ProgramCanRunWithoutArguments)
                    {
                        string programNameLower = this._ProgramName.ToLower();
                        this._Log.Log($"{this._ProgramName} v{this._ProgramVersion}");
                        this._Log.Log($"Run '{programNameLower} help' to get help about the usage.");
                        this._Log.Log($"Run '{programNameLower} info' to get information about the program.");
                        result = 0;
                    }
                    else
                    {
                        this._Mains.OriginalArguments = arguments;
                        this._Mains.OriginalArgumentsAsString = argumentsAsString;
                        this._Mains._Logger = this._Log;
                        this._Mains._SentenceBuilder = this._SentenceBuilder;
                        this._Mains.ApplicationInitialInformation = this._GRYConsoleApplicationInitialInformation;
                        Parser parser = new Parser((settings) =>
                        {
                            settings.CaseInsensitiveEnumValues = true;
                            settings.CaseSensitive = false;
                            settings.AutoHelp = false;
                            settings.AutoVersion = false;
                        });
                        ParserResult<object> parsed = parser.ParseArguments(arguments, this._Mains.GetVerbs());
                        return this._Mains.Run(parsed);
                    }
                }
                catch (Exception exception)
                {
                    this._Log.Log($"Fatal error occurred while processing argument.", exception);
                }
            }
            catch (Exception exception)
            {
                this._Log.Log($"Fatal error occurred", exception);
                result = 2;
            }
            this._Log.Log($"Finished program", LogLevel.Debug);
            return result;
        }
    }
}
