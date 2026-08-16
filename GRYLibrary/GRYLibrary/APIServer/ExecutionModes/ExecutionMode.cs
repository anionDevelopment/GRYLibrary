namespace GRYLibrary.Core.APIServer.ExecutionModes
{
    public abstract class ExecutionMode
    {
        public abstract void Accept(IExecutionModeVisitor visitor);
        public abstract T Accept<T>(IExecutionModeVisitor<T> visitor);
        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
    public interface IExecutionModeVisitor
    {
        void Handle(Analysis analysis);
        void Handle(RunProgram runProgram);
        void Handle(TestRun testRun);
    }
    public interface IExecutionModeVisitor<T>
    {
        T Handle(Analysis analysis);
        T Handle(RunProgram runProgram);
        T Handle(TestRun testRun);
    }
}