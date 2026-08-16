namespace GRYLibrary.Core.APIServer.ExecutionModes
{
    public class TestRun : ExecutionMode
    {
        public static TestRun Instance { get; } = new TestRun();
        private TestRun() { }
        public override void Accept(IExecutionModeVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IExecutionModeVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}
