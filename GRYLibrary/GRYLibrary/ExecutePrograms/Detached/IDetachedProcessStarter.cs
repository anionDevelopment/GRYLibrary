namespace GRYLibrary.Core.ExecutePrograms.Detached
{
    /// <summary>
    /// Starts an external program detached from the current process, so that the operating-system does not treat the
    /// started process as a child-process of the current process and the started process keeps running independently
    /// after the current process ends.
    /// </summary>
    internal interface IDetachedProcessStarter
    {
        /// <summary>
        /// Starts <paramref name="program"/> detached and returns the process-id of the started process.
        /// </summary>
        /// <param name="program">The resolved full path of the program to start.</param>
        /// <param name="argument">The argument-string for the program (same quoting-rules as for an owned execution).</param>
        /// <param name="workingDirectory">The working-directory for the started program.</param>
        /// <param name="createWindow">Whether the started program may create its own window (honored on operating-systems where it applies).</param>
        /// <returns>The process-id of the started process.</returns>
        int StartDetached(string program, string argument, string workingDirectory, bool createWindow);
    }
}
