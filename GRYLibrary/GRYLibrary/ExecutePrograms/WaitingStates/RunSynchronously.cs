namespace GRYLibrary.Core.ExecutePrograms.WaitingStates
{
    public class RunSynchronously : WaitingState
    {
        public bool ThrowErrorIfExitCodeIsNotZero { get; set; } = true;
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