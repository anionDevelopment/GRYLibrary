using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRYLibrary.Core.Misc
{
    public static class GitTools
    {
        public static GitCommandResult ExecuteGitCommand(string repositoryFolder, string argument, bool throwErrorIfExitCodeIsNotZero = false, int? timeoutInMilliseconds = null, bool printErrorsAsInformation = false, bool writeOutputToConsole = false)
        {
            using GRYLog log = GRYLog.Create();
            log.Configuration.Enabled = true;
            log.Configuration.SetEnabledOfAllLogTargets(writeOutputToConsole);
            using ExternalProgramExecutor externalProgramExecutor = new("git", argument, repositoryFolder);
            externalProgramExecutor.Configuration.TimeoutInMilliseconds = timeoutInMilliseconds;
            externalProgramExecutor.Configuration.PrintErrorsAsInformation = printErrorsAsInformation;
            externalProgramExecutor.Configuration.WaitingState = new RunSynchronously() { ThrowErrorIfExitCodeIsNotZero = throwErrorIfExitCodeIsNotZero };
            externalProgramExecutor.Run();
            return new GitCommandResult(argument, repositoryFolder, externalProgramExecutor.AllStdOutLines, externalProgramExecutor.AllStdErrLines, externalProgramExecutor.ExitCode);
        }
        /// <returns>
        /// Returns a enumeration of the submodule-paths of <paramref name="repositoryFolder"/>.
        /// </returns>
        public static IEnumerable<string> GetGitSubmodulePaths(string repositoryFolder, bool recursive = true)
        {
            GitCommandResult commandresult = ExecuteGitCommand(repositoryFolder, "submodule status" + (recursive ? " --recursive" : string.Empty), true);
            List<string> result = [];
            foreach (string rawLine in commandresult.StdOutLines)
            {
                string line = rawLine.Trim();
                if (line.Contains(' '))
                {
                    string[] splitted = line.Split(' ');
                    int amountOfWhitespaces = splitted.Length - 1;
                    if (0 < amountOfWhitespaces)
                    {
                        string rawPath = splitted[1];
                        if (rawPath.Contains("..") || rawPath == "./")
                        {
                            continue;
                        }
                        else
                        {
                            result.Add(Path.Combine(repositoryFolder, rawPath.Replace("/", Path.DirectorySeparatorChar.ToString())));
                        }
                    }
                }
            }
            return result;
        }
        public static bool GitRepositoryContainsObligatoryFiles(string repositoryFolder, out ISet<string> missingFiles)
        {
            List<Tuple<string, ISet<string>>> fileLists =
            [
                Tuple.Create<string, ISet<string>>(".gitignore", new HashSet<string>()),
                Tuple.Create<string, ISet<string>>("License.txt", new HashSet<string>() { "License", "License.md" }),
                Tuple.Create<string, ISet<string>>("ReadMe.md", new HashSet<string>() { "ReadMe", "ReadMe.txt" })
            ];
            return GitRepositoryContainsFiles(repositoryFolder, out missingFiles, fileLists);
        }
        public static void AddGitRemote(string repositoryFolder, string remoteFolder, string remoteName)
        {
            ExecuteGitCommand(repositoryFolder, $"remote add {remoteName} \"{remoteFolder}\"", true);
        }

        public static bool GitRemoteIsAvailable(string repositoryFolder, string remoteName)
        {
            try
            {
                return ExecuteGitCommand(repositoryFolder, $"ls-remote {remoteName}", false, 1000 * 60, writeOutputToConsole: false).ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        /// <returns>Returns the address of the remote with the given <paramref name="remoteName"/>.</returns>
        public static string GetGitRemoteAddress(string repository, string remoteName)
        {
            return ExtractTextFromOutput(ExecuteGitCommand(repository, $"config --get remote.{remoteName}.url", true).StdOutLines);
        }

        public static void SetGitRemoteAddress(string repositoryFolder, string remoteName, string newRemoteAddress)
        {
            ExecuteGitCommand(repositoryFolder, $"remote set-url {remoteName} {newRemoteAddress}", true);
        }

        /// <summary>Removes unused internal files in the .git-folder of the given <paramref name="repositoryFolder"/>.</summary>
        /// <remarks>Warning: After executing this function deleted commits can not be restored because then they are really deleted.</remarks>
        public static void GitTidyUp(string repositoryFolder, bool writeOutputToConsole = false, bool repack = true)
        {
            ExecuteGitCommand(repositoryFolder, $"reflog expire --expire-unreachable=now --all", true, writeOutputToConsole: writeOutputToConsole);
            ExecuteGitCommand(repositoryFolder, $"gc --prune=now", true, writeOutputToConsole: writeOutputToConsole);
            if (repack)
            {
                ExecuteGitCommand(repositoryFolder, $"repack -a -d -n --max-pack-size=10m", true, writeOutputToConsole: writeOutputToConsole);
            }
        }
        public static bool GitRepositoryContainsFiles(string repositoryFolder, out ISet<string> missingFiles, IEnumerable<Tuple<string/*file*/, ISet<string>/*aliase*/>> fileLists)
        {
            missingFiles = new HashSet<string>();
            foreach (Tuple<string, ISet<string>> file in fileLists)
            {
                if (!(File.Exists(Path.Combine(repositoryFolder, file.Item1)) || AtLeastOneFileExistsInFolder(repositoryFolder, file.Item2, out string _)))
                {
                    missingFiles.Add(file.Item1);
                }
            }
            return missingFiles.Count == 0;
        }
        /// <returns>
        /// Returns the names of all remotes of the given <paramref name="repositoryFolder"/>.
        /// </returns>
        /// <remarks>
        /// This function does not return the addresses of these remotes.
        /// </remarks>
        public static IEnumerable<string> GetAllGitRemotes(string repositoryFolder)
        {
            return ExecuteGitCommand(repositoryFolder, "remote", true).StdOutLines.Where(line => !string.IsNullOrWhiteSpace(line));
        }

        public static bool AtLeastOneFileExistsInFolder(string repositoryFolder, IEnumerable<string> files, out string foundFile)
        {
            foreach (string file in files)
            {
                if (File.Exists(Path.Combine(repositoryFolder, file)))
                {
                    foundFile = file;
                    return true;
                }
            }
            foundFile = null;
            return false;
        }

        private static readonly char[] _Separators = ['/'];

        /// <returns>
        /// Returns a tuple.
        /// tuple.Item1 represents the remote-name.
        /// tuple.Item1 represents the remote-branchname.
        /// </returns>
        public static IEnumerable<Tuple<string/*remote-name*/, string/*branch-name*/>> GetAllGitRemoteBranches(string repository)
        {
            return ExecuteGitCommand(repository, "branch -r", true).StdOutLines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line =>
            {
                if (line.Contains('/'))
                {
                    string[] splitted = line.Split(_Separators, 2);
                    return new Tuple<string, string>(splitted[0].Trim(), splitted[1].Trim());
                }
                else
                {
                    throw new Exception($"'{repository}> git branch -r' contained the unexpected output-line '{line}'.");
                }
            });
        }

        /// <returns>Returns the names of the remotes of the given <paramref name="repositoryFolder"/>.</returns>
        public static IEnumerable<string> GetGitRemotes(string repositoryFolder)
        {
            return ExecuteGitCommand(repositoryFolder, "remote", true).StdOutLines.Where(line => !string.IsNullOrWhiteSpace(line));
        }

        public static void RemoveGitRemote(string repositoryFolder, string remote)
        {
            ExecuteGitCommand(repositoryFolder, $"remote remove {remote}", true);
        }

        public static IEnumerable<string> GetLocalGitBranchNames(string repositoryFolder)
        {
            return ExecuteGitCommand(repositoryFolder, "branch", true).StdOutLines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Replace("*", string.Empty).Trim());
        }

        /// <returns>Returns the toplevel of the <paramref name="repositoryFolder"/>.</returns>
        public static string GetTopLevelOfGitRepositoryPath(string repositoryFolder)
        {
            if (IsInGitRepository(repositoryFolder))
            {
                return ExtractTextFromOutput(ExecuteGitCommand(repositoryFolder, "rev-parse --show-toplevel", true).StdOutLines);
            }
            else
            {
                throw new ArgumentException($"The given folder '{repositoryFolder}' is not a git-repository.");
            }
        }
        private static string ExtractTextFromOutput(string[] lines)
        {
            return string.Join(string.Empty, lines).Trim();
        }

        /// <returns>Returns true if and only if <paramref name="repositoryFolder"/> is in a repository which is used as submodule.</returns>
        public static bool IsInGitSubmodule(string repositoryFolder)
        {
            if (IsInGitRepository(repositoryFolder))
            {
                return !GetParentGitRepositoryPathHelper(repositoryFolder).Equals(string.Empty);
            }
            else
            {
                return false;
            }
        }

        /// <returns>
        /// If <paramref name="repositoryFolder"/> is used as submodule then this function returns the toplevel-folder of the parent-repository.
        /// </returns>
        public static string GetParentGitRepositoryPath(string repositoryFolder)
        {
            if (IsInGitRepository(repositoryFolder))
            {
                string content = GetParentGitRepositoryPathHelper(repositoryFolder);
                if (string.IsNullOrEmpty(content))
                {
                    throw new ArgumentException($"The given folder '{repositoryFolder}' is not used as submodule so a parent-repository-path can not be calculated.");
                }
                else
                {
                    return content;
                }
            }
            else
            {
                throw new ArgumentException($"The given folder '{repositoryFolder}' is not a git-repository.");
            }
        }
        private static string GetParentGitRepositoryPathHelper(string repositoryFolder)
        {
            return ExtractTextFromOutput(ExecuteGitCommand(repositoryFolder, "rev-parse --show-superproject-working-tree", true).StdOutLines);
        }

        /// <returns>
        /// Returns true if and only if <paramref name="folder"/> is the toplevel-folder of a git-repository.
        /// </returns>
        public static bool IsGitRepository(string folder)
        {
            string gitPrefix = ".git";
            string combinedPath = Path.Combine(folder, gitPrefix);
            if (Directory.Exists(combinedPath))
            {
                return true;//usual repository
            }
            if (File.Exists(combinedPath))
            {
                return true;//usual repository where the .git-folder somewhere else (often used for example in submodules)
            }
            if (folder.EndsWith(gitPrefix))
            {
                return true;//convention for a repository-folder-name of a bare is met.
            }
            return false;
        }
        /// <returns>
        /// Returns true if and only if <paramref name="folder"/> or a parent-folder of <paramref name="folder"/> is a toplevel-folder of a git-repository.
        /// </returns>
        public static bool IsInGitRepository(string folder)
        {
            DirectoryInfo directoryInfo = new(folder);
            if (IsGitRepository(directoryInfo.FullName))
            {
                return true;
            }
            else if (directoryInfo.Parent == null)
            {
                return false;
            }
            else
            {
                return IsInGitRepository(directoryInfo.Parent.FullName);
            }
        }
        /// <summary>
        /// Commits all staged and unstaged changes in <paramref name="repositoryFolder"/>.
        /// </summary>
        /// <param name="repositoryFolder">Repository where changes should be committed</param>
        /// <param name="commitMessage">Message for the commit</param>
        /// <param name="commitWasCreated">Will be set to true if and only if really a commit was created. Will be set to false if and only if there are no changes to get committed.</param>
        /// <returns>Returns the commit-id of the currently checked out commit. This returns the id of the new created commit if there were changes which were committed by this function.</returns>
        /// <exception cref="Exceptions.UnexpectedExitCodeException">If there are uncommitted changes in submodules of <paramref name="repositoryFolder"/>.</exception>
        public static string GitCommit(string repositoryFolder, string commitMessage, out bool commitWasCreated, bool writeOutputToConsole = false)
        {
            commitWasCreated = false;
            if (GitRepositoryHasUncommittedChanges(repositoryFolder))
            {
                ExecuteGitCommand(repositoryFolder, $"add -A", true, writeOutputToConsole: writeOutputToConsole);
                ExecuteGitCommand(repositoryFolder, $"commit -m \"{commitMessage}\"", true, writeOutputToConsole: writeOutputToConsole);
                commitWasCreated = true;
            }
            return GetLastGitCommitId(repositoryFolder, "HEAD", writeOutputToConsole);
        }
        /// <returns>Returns the commit-id of the given <paramref name="revision"/>.</returns>
        public static string GetLastGitCommitId(string repositoryFolder, string revision = "HEAD", bool writeOutputToConsole = false)
        {
            return ExecuteGitCommand(repositoryFolder, $"rev-parse {revision}", true, writeOutputToConsole: writeOutputToConsole).GetFirstStdOutLine();
        }

        /// <param name="printErrorsAsInformation">
        /// Represents a value which indicates if the git-output which goes to stderr should be treated as stdout.
        /// The default-value is true since even if no error occurs git write usual information to stderr.
        /// If really an error occures (=the exit-code of git is not 0) then this function throws an exception
        /// </param>
        public static void GitFetch(string repositoryFolder, string remoteName = "--all", bool printErrorsAsInformation = true, bool writeOutputToConsole = false)
        {
            ExecuteGitCommand(repositoryFolder, $"fetch {remoteName} --tags --prune", true, printErrorsAsInformation: printErrorsAsInformation, writeOutputToConsole: writeOutputToConsole);
        }

        public static bool GitRepositoryHasUnstagedChanges(string repositoryFolder)
        {
            if (GitRepositoryHasUnstagedChangesOfTrackedFiles(repositoryFolder))
            {
                return true;
            }
            if (GitRepositoryHasNewUntrackedFiles(repositoryFolder))
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<string> GetFilesOfGitRepository(string repositoryFolder, string revision)
        {
            return ExecuteGitCommand(repositoryFolder, $"ls-tree --full-tree -r --name-only {revision}", true).StdOutLines;
        }

        public static bool GitRepositoryHasNewUntrackedFiles(string repositoryFolder)
        {
            return GitChangesHelper(repositoryFolder, "ls-files --exclude-standard --others");
        }

        public static bool GitRepositoryHasUnstagedChangesOfTrackedFiles(string repositoryFolder)
        {
            return GitChangesHelper(repositoryFolder, "diff");
        }

        public static bool GitRepositoryHasStagedChanges(string repositoryFolder)
        {
            return GitChangesHelper(repositoryFolder, "diff --cached");
        }

        public static void GitRenameBranch(string repositoryFolder, string oldBranchName, string newBranchName)
        {
            ExecuteGitCommand(repositoryFolder, $"branch -m {oldBranchName} {newBranchName}", true);
        }

        private static bool GitChangesHelper(string repositoryFolder, string argument)
        {
            GitCommandResult result = ExecuteGitCommand(repositoryFolder, argument, true);
            foreach (string line in result.StdOutLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GitRepositoryHasUncommittedChanges(string repositoryFolder)
        {
            if (GitRepositoryHasUnstagedChanges(repositoryFolder))
            {
                return true;
            }
            if (GitRepositoryHasStagedChanges(repositoryFolder))
            {
                return true;
            }
            return false;
        }
        /// <remarks>
        /// <paramref name="revision"/> can be all kinds of revision-labels, for example "HEAD" or branch-names (e. g. "master") oder revision-ids (e. g. "a1b2c3b4").
        /// </remarks>
        public static int GetAmountOfCommitsInGitRepository(string repositoryFolder, string revision = "HEAD")
        {
            return int.Parse(ExecuteGitCommand(repositoryFolder, $"rev-list --count {revision}", true).GetFirstStdOutLine());
        }

        public static string GetCurrentGitRepositoryBranch(string repositoryFolder)
        {
            return ExecuteGitCommand(repositoryFolder, $"rev-parse --abbrev-ref HEAD", true).GetFirstStdOutLine();
        }

        /// <remarks>
        /// <paramref name="ancestor"/> and <paramref name="descendant"/> can be all kinds of revision-labels, for example "HEAD" or branch-names (e. g. "master") oder revision-ids (e. g. "a1b2c3b4").
        /// </remarks>
        public static bool IsGitCommitAncestor(string repositoryFolder, string ancestor, string descendant = "HEAD")
        {
            return ExecuteGitCommand(repositoryFolder, $"merge-base --is-ancestor {ancestor} {descendant}", false).ExitCode == 0;
        }
    }
}