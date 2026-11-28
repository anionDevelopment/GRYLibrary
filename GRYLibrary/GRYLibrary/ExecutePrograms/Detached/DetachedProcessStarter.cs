using System.IO;
using System.Runtime.InteropServices;

namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Selects the <see cref="IDetachedProcessStarter"/>-implementation for the current operating-system.
    /// </summary>
    internal static class DetachedProcessStarter
    {
        internal static IDetachedProcessStarter GetForCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsDetachedProcessStarter();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new UnixDetachedProcessStarter();
            }
            throw new InvalidDataException("Detached process-execution is not supported on the current operating-system.");
        }
    }
}
