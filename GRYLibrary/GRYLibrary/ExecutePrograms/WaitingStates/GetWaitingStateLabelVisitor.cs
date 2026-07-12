namespace GRYLibrary.Core.ExecutePrograms.WaitingStates
{
    public class GetWaitingStateLabelVisitor : IWaitingStateVisitor<string>
    {
        public static IWaitingStateVisitor<string> GetWaitingStateLabelVisitorInstance { get; } = new GetWaitingStateLabelVisitor();
        private GetWaitingStateLabelVisitor() { }
        public string Handle(RunAsynchronously runAsynchronously)
        {
            return "asynchronously";
        }

        public string Handle(RunSynchronously runSynchronously)
        {
            return "synchronously";
        }
    }
}