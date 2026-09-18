using System.Runtime.InteropServices;

namespace GRYLibrary.Core.OperatingSystem
{
    /// <summary>
    /// Contains the functions of the c-standard-library which are required to implement the operating-system-specific behaviour of unix-based operating-systems.
    /// </summary>
    /// <remarks>
    /// On <see cref="ConcreteOperatingSystems.Linux"/> "libc" is resolved to the installed c-standard-library and on <see cref="ConcreteOperatingSystems.OSX"/> it is resolved to "libSystem.dylib".
    /// Therefore these functions must only be called if the current operating-system is unix-based.
    /// </remarks>
    internal static class UnixNativeMethods
    {
        /// <returns>
        /// Returns the effective user-id of the current process. The user-id 0 belongs to the user "root".
        /// </returns>
        [DllImport("libc", EntryPoint = "geteuid")]
        internal static extern uint GetEffectiveUserId();
    }
}