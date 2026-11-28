using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Starts a program detached on Linux and macOS using <c>posix_spawn</c> with the <c>POSIX_SPAWN_SETSID</c>-flag, so
    /// that the started process becomes the leader of a new session (detached from the executor's process-group and
    /// controlling-terminal) and keeps running independently after the executor ends.
    /// </summary>
    /// <remarks>
    /// <c>POSIX_SPAWN_SETSID</c> requires glibc 2.26 or later on Linux and macOS 10.15 or later; its numeric value
    /// differs between the two platforms. Setting the working-directory requires <c>posix_spawn_file_actions_addchdir_np</c>
    /// (glibc 2.29 or later, macOS 10.15 or later).
    /// </remarks>
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("osx")]
    internal sealed class UnixDetachedProcessStarter : IDetachedProcessStarter
    {
        private const short PosixSpawnSetsidLinux = 0x0080;
        private const short PosixSpawnSetsidMacOs = 0x0400;
        private const int NativeStructureBufferSize = 1024;

        public int StartDetached(string program, string argument, string workingDirectory, bool createWindow)
        {
            short setsidFlag = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? PosixSpawnSetsidMacOs : PosixSpawnSetsidLinux;
            string[] argumentVector = BuildArgumentVector(program, argument);
            string[] environmentVector = BuildEnvironmentVector();
            IntPtr attributes = Marshal.AllocHGlobal(NativeStructureBufferSize);
            IntPtr fileActions = Marshal.AllocHGlobal(NativeStructureBufferSize);
            try
            {
                ZeroMemory(attributes, NativeStructureBufferSize);
                ZeroMemory(fileActions, NativeStructureBufferSize);
                CheckNativeResult(UnixSpawnNativeMethods.PosixSpawnAttributesInitialize(attributes), nameof(UnixSpawnNativeMethods.PosixSpawnAttributesInitialize), program);
                try
                {
                    CheckNativeResult(UnixSpawnNativeMethods.PosixSpawnAttributesSetFlags(attributes, setsidFlag), nameof(UnixSpawnNativeMethods.PosixSpawnAttributesSetFlags), program);
                    CheckNativeResult(UnixSpawnNativeMethods.PosixSpawnFileActionsInitialize(fileActions), nameof(UnixSpawnNativeMethods.PosixSpawnFileActionsInitialize), program);
                    try
                    {
                        CheckNativeResult(UnixSpawnNativeMethods.PosixSpawnFileActionsAddChangeDirectory(fileActions, workingDirectory), nameof(UnixSpawnNativeMethods.PosixSpawnFileActionsAddChangeDirectory), program);
                        CheckNativeResult(UnixSpawnNativeMethods.PosixSpawn(out int processId, program, fileActions, attributes, argumentVector, environmentVector), nameof(UnixSpawnNativeMethods.PosixSpawn), program);
                        return processId;
                    }
                    finally
                    {
                        UnixSpawnNativeMethods.PosixSpawnFileActionsDestroy(fileActions);
                    }
                }
                finally
                {
                    UnixSpawnNativeMethods.PosixSpawnAttributesDestroy(attributes);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(fileActions);
                Marshal.FreeHGlobal(attributes);
            }
        }

        private static string[] BuildArgumentVector(string program, string argument)
        {
            List<string> argumentVector = new() { program };
            foreach (string singleArgument in CommandLineArgumentParser.Parse(argument))
            {
                argumentVector.Add(singleArgument);
            }
            argumentVector.Add(null); // posix_spawn requires a null-terminated argument-vector.
            return argumentVector.ToArray();
        }

        private static string[] BuildEnvironmentVector()
        {
            List<string> environmentVector = new();
            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
            {
                environmentVector.Add($"{environmentVariable.Key}={environmentVariable.Value}");
            }
            environmentVector.Add(null); // posix_spawn requires a null-terminated environment-vector.
            return environmentVector.ToArray();
        }

        private static void CheckNativeResult(int nativeResult, string nativeFunctionName, string program)
        {
            if (nativeResult != 0)
            {
                throw new InvalidOperationException($"The detached process '{program}' could not be started: '{nativeFunctionName}' returned the error-number {nativeResult}.");
            }
        }

        private static void ZeroMemory(IntPtr buffer, int length)
        {
            for (int offset = 0; offset < length; offset++)
            {
                Marshal.WriteByte(buffer, offset, 0);
            }
        }
    }
}
