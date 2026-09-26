using GRYLibrary.Core.Misc;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.CustomDisposables;
using GRYLibrary.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Assert.HasCount(1, externalProgramExecutor.AllStdOutLines);
            Assert.AreEqual(testStdOut, externalProgramExecutor.AllStdOutLines[0]);
            Assert.IsEmpty(externalProgramExecutor.AllStdErrLines);
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
            Assert.HasCount(expectedLines.Length + 1, actualLines);
            for (int i = 0; i < expectedLines.Length; i++)
            {
                Assert.AreEqual(expectedLines[i], actualLines[i]);
            }
            // The last line contains the (non-deterministic) execution-duration.
            Assert.StartsWith("Execution-duration: ", actualLines[^1]);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestTerminateEndsAnAsynchronouslyExecutedProgram()
        {
            //arrange
            ExternalProgramExecutor externalProgramExecutor = CreateExecutorForALongRunningAsynchronousProgram();
            externalProgramExecutor.Run();
            int processId = externalProgramExecutor.ProcessId;
            Assert.IsTrue(ProcessIsRunning(processId));

            //act
            externalProgramExecutor.Terminate();

            //assert
            Assert.IsFalse(ProcessIsRunning(processId));
            Assert.IsFalse(externalProgramExecutor.IsRunning);
            Assert.AreEqual(ExecutionState.Terminated, externalProgramExecutor.CurrentExecutionState);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestDisposeEndsAnAsynchronouslyExecutedProgram()
        {
            //arrange
            int processId;

            //act
            using (ExternalProgramExecutor externalProgramExecutor = CreateExecutorForALongRunningAsynchronousProgram())
            {
                externalProgramExecutor.Run();
                processId = externalProgramExecutor.ProcessId;
                Assert.IsTrue(ProcessIsRunning(processId));
            }

            //assert
            Assert.IsFalse(ProcessIsRunning(processId));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestTerminateAnAlreadyEndedProgramDoesNothing()
        {
            //arrange
            ExternalProgramExecutor externalProgramExecutor = CreateExecutorForALongRunningAsynchronousProgram();
            externalProgramExecutor.Run();
            externalProgramExecutor.Terminate();

            //act
            externalProgramExecutor.Terminate();

            //assert
            Assert.AreEqual(ExecutionState.Terminated, externalProgramExecutor.CurrentExecutionState);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestTerminateANotStartedProgramThrowsAnException()
        {
            //arrange
            ExternalProgramExecutor externalProgramExecutor = CreateExecutorForALongRunningAsynchronousProgram();

            //act and assert
            Assert.Throws<InvalidOperationException>(externalProgramExecutor.Terminate);
        }

        /// <remarks>
        /// The execution of an asynchronously executed program has to be completed when the program ends on its own,
        /// because otherwise its result would never become available and the resources of the execution would never be
        /// released.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestAsynchronousExecutionIsCompletedWhenTheProgramEndsOnItsOwn()
        {
            //arrange
            // See the remark in TestVerboseExecutionProducesExpectedStdOutLogSequence regarding the used echo-program.
            string echoProgram = System.OperatingSystem.IsWindows() ? "echo2" : "echo";
            ExternalProgramExecutor externalProgramExecutor = new(new ExternalProgramExecutorConfiguration()
            {
                Program = echoProgram,
                Argument = "x",
                WaitingState = new RunAsynchronously(),
            });

            //act
            externalProgramExecutor.Run();

            //assert
            // The timeout is set explicitly (and much shorter than the default-timeout) so that this testcase fails
            // instead of blocking the testrun if the execution is not completed anymore.
            Core.Misc.Utilities.WaitUntilConditionIsTrue(() => externalProgramExecutor.CurrentExecutionState == ExecutionState.Terminated, TimeSpan.FromSeconds(30), "Wait until the asynchronous execution is completed");
            Assert.IsFalse(externalProgramExecutor.IsRunning);
            Assert.AreEqual(0, externalProgramExecutor.ExitCode);
            Assert.AreEqual("x", externalProgramExecutor.AllStdOutLines.Single());
        }

        private static ExternalProgramExecutor CreateExecutorForALongRunningAsynchronousProgram()
        {
            (string program, string argument) = TestUtilities.GetLongRunningProgram(60);
            return new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration()
            {
                Program = program,
                Argument = argument,
                WaitingState = new RunAsynchronously(),
            });
        }

        /// <remarks>
        /// This asks the operating-system and not the <see cref="ExternalProgramExecutor"/>, because the point of the
        /// testcases which use this is that the executed program itself is really ended and not only that the execution
        /// is bookkept as ended.
        /// </remarks>
        private static bool ProcessIsRunning(int processId)
        {
            Process[] processes = Process.GetProcesses();
            try
            {
                return processes.Any(process => process.Id == processId);
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }
        }
    }
}