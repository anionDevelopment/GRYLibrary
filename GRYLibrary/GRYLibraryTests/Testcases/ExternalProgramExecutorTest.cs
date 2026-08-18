using GRYLibrary.Core.Misc;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.CustomDisposables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class ExternalProgramExecutorTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestEchoWithSomeSpecialCharacter()
        {
            string testStdOut = "test \\ \" < > ' testend";
            ExternalProgramExecutor externalProgramExecutor = new("echo", '"' + testStdOut.Replace("\"", "\\\"") + '"');
            externalProgramExecutor.Run();
            Assert.AreEqual(0, externalProgramExecutor.ExitCode);
            Assert.AreEqual(1, externalProgramExecutor.AllStdOutLines.Length);
            Assert.AreEqual(testStdOut, externalProgramExecutor.AllStdOutLines[0]);
            Assert.AreEqual(0, externalProgramExecutor.AllStdErrLines.Length);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestCopyFileWithSpaceInFilename()
        {
            //arrange
            using TemporaryDirectory temporaryDirectory = new();
            string file1name = "File 1.txt";
            string file1 = Path.Combine(temporaryDirectory.TemporaryDirectoryPath, file1name);
            Core.Misc.Utilities.EnsureFileExists(file1);
            string file2name = "File 2.txt";
            string file2 = Path.Combine(temporaryDirectory.TemporaryDirectoryPath, file2name);
            Core.Misc.Utilities.AssertCondition(!File.Exists(file2));
            ExternalProgramExecutor externalProgramExecutor = new("cp", $"\"{file1name}\" \"{file2name}\"", temporaryDirectory.TemporaryDirectoryPath);

            //act
            externalProgramExecutor.Run();

            //assert
            Assert.IsTrue(File.Exists(file2));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestCopyFileUseUmlautsAndOtherCharacterFromOtherLanguages()
        {
            //arrange
            using TemporaryDirectory temporaryDirectory = new();
            string file1name = "Sourcefile.txt";
            string file1 = Path.Combine(temporaryDirectory.TemporaryDirectoryPath, file1name);
            Core.Misc.Utilities.EnsureFileExists(file1);
            string file2name = "[SpecialCharacterTest]äöüßÄÖ'ÜÆÑçéý[_SpecialCharacterTest].txt";
            string file2 = Path.Combine(temporaryDirectory.TemporaryDirectoryPath, file2name);
            Core.Misc.Utilities.AssertCondition(!File.Exists(file2));
            ExternalProgramExecutor externalProgramExecutor = new("cp", $"\"{file1name}\" \"{file2name}\"", temporaryDirectory.TemporaryDirectoryPath);

            //act
            externalProgramExecutor.Run();

            //assert
            Assert.IsTrue(File.Exists(file2));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestVerboseExecutionProducesExpectedStdOutLogSequence()
        {
            //arrange
            using TemporaryDirectory temporaryDirectory = new();
            GRYLog logObject = GRYLog.Create();
            logObject.Configuration.Initliaze();
            logObject.Configuration.StoreProcessedLogItemsInternally = true;
            // On Windows "echo" is a built-in cmd-command and not an executable, so the test relies on "echo2" (a program which is expected to be available on developer-machines; see Hints.md).
            // On Linux and macOS the regular "echo"-executable is used.
            string echoProgram = System.OperatingSystem.IsWindows() ? "echo2" : "echo";
            ExternalProgramExecutor externalProgramExecutor = new(new ExternalProgramExecutorConfiguration()
            {
                Program = echoProgram,
                Argument = "x",
                WorkingDirectory = temporaryDirectory.TemporaryDirectoryPath,
                Verbosity = Verbosity.Verbose,
            })
            {
                LogObject = logObject
            };

            //act
            externalProgramExecutor.Run();

            //assert
            Assert.AreEqual(0, externalProgramExecutor.ExitCode);
            // The program-path and the working-directory get resolved to their full paths and the process-id is only known after the start.
            // Therefore the expected output gets built from the actually resolved values so that this test stays machine-independent.
            string resolvedProgram = externalProgramExecutor.Configuration.Program;
            string resolvedWorkingDirectory = externalProgramExecutor.Configuration.WorkingDirectory;
            int processId = externalProgramExecutor.ProcessId;
            string commandLine = $"{resolvedWorkingDirectory}>{resolvedProgram} x";

            List<string> actualLines = logObject.ProcessedLogItems.Select(logItem => logItem.PlainMessage).ToList();
            string[] expectedLines =
            [
                $"Program to execute with full path: {resolvedProgram}",
                "Program will be executed synchronously",
                "Start executing program",
                $"Program which will be executed: {commandLine}",
                $"Process-Id of started program: {processId}",
                $"Output-lines:",
                "x",
                "Finished executing program.",
                "ExternalProgramExecutor-summary:",
                "Title: ",
                $"Executed program: {commandLine}",
                $"Process-Id: {processId}",
                "Exit-code: 0",
            ];
            Assert.AreEqual(expectedLines.Length + 1, actualLines.Count);
            for (int i = 0; i < expectedLines.Length; i++)
            {
                Assert.AreEqual(expectedLines[i], actualLines[i]);
            }
            // The last line contains the (non-deterministic) execution-duration.
            Assert.IsTrue(actualLines[^1].StartsWith("Execution-duration: "));
        }
    }
}