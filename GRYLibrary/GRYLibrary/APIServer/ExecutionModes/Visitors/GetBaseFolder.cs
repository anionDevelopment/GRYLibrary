using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using System;
using System.Collections;
using System.IO;

namespace GRYLibrary.Core.APIServer.ExecutionModes.Visitors
{
    public class GetBaseFolder : IExecutionModeVisitor<string>
    {
        private readonly GRYEnvironment _TargetEnvironmentType;
        private readonly string _ProgramFolder;
        private readonly bool _IsTestRun;
        public GetBaseFolder(GRYEnvironment targetEnvironmentType, string programFolder, bool isTestRun)
        {
            this._TargetEnvironmentType = targetEnvironmentType;
            this._ProgramFolder = programFolder;
            this._IsTestRun = isTestRun;
        }

        public string Handle(RunProgram runProgram)
        {
            return GetBaseFolderForProjectInCommonProjectStructure(this._TargetEnvironmentType, this._ProgramFolder, runProgram, this._IsTestRun);
        }

        public string Handle(TestRun testRun)
        {
            return this.GetTempFolder();
        }

        public string Handle(Analysis analysis)
        {
            return this.GetTempFolder();
        }

        private string GetTempFolder()
        {
            string result = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            Misc.Utilities.EnsureDirectoryExists(result);
            return result;
        }
        /// <summary>
        /// The name of the environment-variable which states that the application runs in a container. It is written
        /// in upper case, which is the usual form of an environment-variable, so that the name is the same one on a
        /// system which compares such names case-sensitively (linux) and on one which does not (windows).
        /// </summary>
        public const string NameOfTheVariableWhichStatesTheContainer = "ISRUNNINGINCONTAINER";

        /// <summary>
        /// States whether the application runs in a container.
        /// </summary>
        /// <remarks>
        /// The name of the variable is compared without case: an image which sets it as "IsRunningInContainer" states
        /// the same thing as one which sets it as "ISRUNNINGINCONTAINER", and on linux a case-sensitive lookup would
        /// silently answer "no container" for the first one - which moves every folder of the application somewhere
        /// else.
        /// </remarks>
        public static bool IsRunningInContainer()
        {
            foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
            {
                if (variable.Key is string name && name.Equals(NameOfTheVariableWhichStatesTheContainer, StringComparison.OrdinalIgnoreCase))
                {
                    return variable.Value is string value && value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        public static string GetBaseFolderForProjectInCommonProjectStructure(GRYEnvironment environment, string programFolder, ExecutionMode executionMode, bool isTestRun)
        {
            string workspaceFolderName = "Workspace";
            string result;
            if (IsRunningInContainer())
            {
                result = $"/{workspaceFolderName}";//running in container
            }
            else
            {
                result = Misc.Utilities.ResolveToFullPath($"../../{workspaceFolderName}", programFolder);//running locally
            }
            return result;
        }
    }
}