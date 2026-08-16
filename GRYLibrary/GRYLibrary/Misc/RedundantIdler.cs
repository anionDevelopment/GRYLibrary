using System.Diagnostics;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// This type is a helper to start a backup program in a very simple way:
    /// In the First line of the entry-point of your program (e.g. "Main()" in Program.cs) you simply call "<see cref="RedundantIdler"/>.<see cref="WaitUntilExecutionIsRequired"/>();".
    /// This will start a second instance of the program with the same working-directory and same arguments. This second instance waits and does nothing until the first instance of the
    /// program terminated. When the first instance of the program terminated the second instance will now be treated as first instance so it starts a new backup-instance and runs
    /// the program quite normally.
    /// </summary>
    /// <example>
    /// The name of your program is MyProgram.exe. MyProgram.exe provides an important service. So when you start this program you want to ensure that MyProgram.exe is always running
    /// even if MyProgram.exe crashes or someone kills MyProgram.exe.
    /// To have an easy way which ensures that MyProgram.exe will always be restarted if it will be terminated for any reason is using <see cref="RedundantIdler"/>.
    /// <see cref="RedundantIdler"/> starts a second instance which sleeps/waits exactly until the first instance is terminated.
    /// </example>
    /// <remarks>
    /// To identify if a program using <see cref="RedundantIdler"/> is the first or the second instance only the name of the process will be compared. <see cref="RedundantIdler"/> does only work if
    /// there is always exactly one process running on the computer which the name of the process which is using <see cref="RedundantIdler"/>.
    /// Currently there is no clean way to terminate both processes.
    /// </remarks>
    internal/*Internal because implementation not tested yet*/ sealed class RedundantIdler
    {
        //TODO change the identification for first/second intance to a system that does not compare process-names:
        //If you start MyProgram.exe twice manually then both of them should run independently and have their own backup-instance.
        private RedundantIdler() { }

        public static int? ProcessIdOfBackupProcess { get; private set; } = null;
        public static void WaitUntilExecutionIsRequired()
        {
            if (ThisProgramIsTheOnlyInstance())
            {
                StartBackupInstance();
            }
            else
            {
                WaitUntilOtherProgramFinished();
                StartBackupInstance();
            }
        }

        private static void WaitUntilOtherProgramFinished()
        {
            while (!ThisProgramIsTheOnlyInstance())
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        private static void StartBackupInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            using Process backupProcess = new()
            {
                StartInfo = currentProcess.StartInfo//TODO test if this works
            };
            backupProcess.Start();
            ProcessIdOfBackupProcess = backupProcess.Id;
        }

        private static bool ThisProgramIsTheOnlyInstance()
        {
            return GetAmountOfProcessesWithNameOfCurrentProcess() == 1;
        }

        private static int GetAmountOfProcessesWithNameOfCurrentProcess()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            return Process.GetProcessesByName(currentProcessName).Length;
        }

        public static void TerminateSecondInstance()
        {
            if (ProcessIdOfBackupProcess.HasValue && Process.GetProcessById(ProcessIdOfBackupProcess.Value) != null)
            {
                Process.GetProcessById(ProcessIdOfBackupProcess.Value).Kill();
            }
            else
            {
                throw new System.Exception("No backup-instance available which could be closed.");
            }
        }
    }
}