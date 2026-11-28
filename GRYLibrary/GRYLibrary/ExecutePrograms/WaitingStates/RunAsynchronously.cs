namespace GRYLibrary.Core.ExecutePrograms.WaitingStates
{
    public class RunAsynchronously : WaitingState
    {
        /// <summary>
        /// When <see langword="true"/> the program is started detached (fire-and-forget): the operating-system does not
        /// treat it as a child-process of the executor and it keeps running independently after the executor is disposed.
        /// A detached execution does not capture the standard-streams or the exit-code, does not enforce a timeout and is
        /// not terminated when the executor is disposed. When <see langword="false"/> the program is executed as an owned
        /// asynchronous process whose result the executor keeps available.
        /// </summary>
        public bool RunDetached { get; set; } = false;
        public override void Accept(IWaitingStateVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IWaitingStateVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}
