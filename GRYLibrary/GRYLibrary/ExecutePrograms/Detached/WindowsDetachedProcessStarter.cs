using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Starts a program detached on Windows using <c>CreateProcessW</c> with detaching creation-flags. The process is
    /// released from the executor's console and process-group and, when the surrounding job allows it, from the job, so
    /// that it is not treated as a child-process of the executor and keeps running after the executor ends.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class WindowsDetachedProcessStarter : IDetachedProcessStarter
    {
        private const uint DetachedProcess = 0x00000008;
        private const uint CreateNewProcessGroup = 0x00000200;
        private const uint CreateBreakawayFromJob = 0x01000000;
        private const int ErrorAccessDenied = 5;

        public int StartDetached(string program, string argument, string workingDirectory, bool createWindow)
        {
            string commandLine = BuildCommandLine(program, argument);
            uint creationFlagsWithBreakaway = DetachedProcess | CreateNewProcessGroup | CreateBreakawayFromJob;
            if (TryCreateProcess(program, commandLine, workingDirectory, creationFlagsWithBreakaway, out int processId, out int lastError))
            {
                return processId;
            }
            if (lastError == ErrorAccessDenied)
            {
                // The surrounding job forbids breakaway. Retry without it: the process is then still detached from the
                // console and process-group but may be bound to the job's lifetime.
                uint creationFlagsWithoutBreakaway = DetachedProcess | CreateNewProcessGroup;
                if (TryCreateProcess(program, commandLine, workingDirectory, creationFlagsWithoutBreakaway, out processId, out lastError))
                {
                    return processId;
                }
            }
            throw new Win32Exception(lastError, $"The detached process '{program}' could not be started.");
        }

        private static bool TryCreateProcess(string program, string commandLine, string workingDirectory, uint creationFlags, out int processId, out int lastError)
        {
            Startupinfo startupInformation = new();
            startupInformation.cb = (uint)Marshal.SizeOf<Startupinfo>();
            bool created = CreateProcess(program, commandLine, IntPtr.Zero, IntPtr.Zero, false, creationFlags, IntPtr.Zero, workingDirectory, ref startupInformation, out ProcessInformation processInformation);
            if (!created)
            {
                lastError = Marshal.GetLastWin32Error();
                processId = 0;
                return false;
            }
            lastError = 0;
            processId = (int)processInformation.dwProcessId;
            CloseHandle(processInformation.hProcess);
            CloseHandle(processInformation.hThread);
            return true;
        }

        private static string BuildCommandLine(string program, string argument)
        {
            string quotedProgram = "\"" + program + "\"";
            if (string.IsNullOrEmpty(argument))
            {
                return quotedProgram;
            }
            return quotedProgram + " " + argument;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Startupinfo
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Startupinfo lpStartupInfo, out ProcessInformation lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
