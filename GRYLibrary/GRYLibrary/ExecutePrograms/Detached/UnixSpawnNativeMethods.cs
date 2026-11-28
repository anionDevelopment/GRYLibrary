using System.Runtime.InteropServices;

namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Contains the <c>posix_spawn</c>-functions of the c-standard-library which are required to start a program detached
    /// on unix-based operating-systems. These functions must only be called on a unix-based operating-system (see
    /// <see cref="GRYLibrary.Core.OperatingSystem.UnixNativeMethods"/> for the library-resolution remark).
    /// </summary>
    internal static class UnixSpawnNativeMethods
    {
        [DllImport("libc", EntryPoint = "posix_spawn", SetLastError = true)]
        internal static extern int PosixSpawn(
            out int processId,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            System.IntPtr fileActions,
            System.IntPtr attributes,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] argumentVector,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] environmentVector);

        [DllImport("libc", EntryPoint = "posix_spawnattr_init", SetLastError = true)]
        internal static extern int PosixSpawnAttributesInitialize(System.IntPtr attributes);

        [DllImport("libc", EntryPoint = "posix_spawnattr_destroy", SetLastError = true)]
        internal static extern int PosixSpawnAttributesDestroy(System.IntPtr attributes);

        [DllImport("libc", EntryPoint = "posix_spawnattr_setflags", SetLastError = true)]
        internal static extern int PosixSpawnAttributesSetFlags(System.IntPtr attributes, short flags);

        [DllImport("libc", EntryPoint = "posix_spawn_file_actions_init", SetLastError = true)]
        internal static extern int PosixSpawnFileActionsInitialize(System.IntPtr fileActions);

        [DllImport("libc", EntryPoint = "posix_spawn_file_actions_destroy", SetLastError = true)]
        internal static extern int PosixSpawnFileActionsDestroy(System.IntPtr fileActions);

        [DllImport("libc", EntryPoint = "posix_spawn_file_actions_addchdir_np", SetLastError = true)]
        internal static extern int PosixSpawnFileActionsAddChangeDirectory(System.IntPtr fileActions, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    }
}
