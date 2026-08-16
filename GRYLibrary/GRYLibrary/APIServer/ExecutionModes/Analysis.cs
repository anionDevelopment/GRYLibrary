namespace GRYLibrary.Core.APIServer.ExecutionModes
{
    public class Analysis : ExecutionMode
    {
        public static Analysis Instance { get; } = new Analysis();
        private Analysis() { }

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