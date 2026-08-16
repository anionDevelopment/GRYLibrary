using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using System;
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
        public static bool IsRunningInContainer()
        {
            string? value = Environment.GetEnvironmentVariable("ISRUNNINGINCONTAINER");

            if (value != null)
            {
                return value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return false;
            }
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