using GRYLibrary.Core.APIServer.Utilities.InitializationStates;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public abstract class InitializationState
    {
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }

        public override string? ToString()
        {
            return this.GetType().Name;
        }
        public abstract void Accept(IInitializationStateVisitor visitor);
        public abstract T Accept<T>(IInitializationStateVisitor<T> visitor);
    }
    public interface IInitializationStateVisitor
    {
        void Handle(InitializationFailed initializationFailed);
        void Handle(Uninitialized uninitialized);
        void Handle(Initializing initializing);
        void Handle(Initialized initialized);
    }
    public interface IInitializationStateVisitor<T>
    {
        T Handle(InitializationFailed initializationFailed);
        T Handle(Uninitialized uninitialized);
        T Handle(Initializing initializing);
        T Handle(Initialized initialized);
    }
}
