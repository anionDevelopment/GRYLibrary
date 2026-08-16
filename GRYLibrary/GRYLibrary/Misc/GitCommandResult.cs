
namespace GRYLibrary.Core.Misc
{
    public class GitCommandResult
    {
        public GitCommandResult()
        {
        }
        public GitCommandResult(string gitArguments, string repository, string[] stdOutLines, string[] stdErrLines, int exitCode)
        {
            this.GitArguments = gitArguments;
            this.Repository = repository;
            this.StdOutLines = stdOutLines;
            this.StdErrLines = stdErrLines;
            this.ExitCode = exitCode;
        }
        public string GitArguments { get; set; }
        public string Repository { get; set; }
        public string[] StdOutLines { get; set; }
        public string[] StdErrLines { get; set; }
        public int ExitCode { get; set; }
        public string GetFirstStdOutLine()
        {
            return this.StdOutLines[0].Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }
    }
}