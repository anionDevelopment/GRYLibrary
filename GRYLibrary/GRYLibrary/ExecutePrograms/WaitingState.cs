using GRYLibrary.Core.ExecutePrograms.WaitingStates;

namespace GRYLibrary.Core.ExecutePrograms
{
    public abstract class WaitingState
    {
        public abstract void Accept(IWaitingStateVisitor visitor);
        public abstract T Accept<T>(IWaitingStateVisitor<T> visitor);
    }
    public interface IWaitingStateVisitor
    {
        void Handle(RunAsynchronously waitingStateVisitor);
        void Handle(RunSynchronously waitingStateVisitor);
    }
    public interface IWaitingStateVisitor<T>
    {
        T Handle(RunAsynchronously waitingStateVisitor);
        T Handle(RunSynchronously waitingStateVisitor);
    }
}