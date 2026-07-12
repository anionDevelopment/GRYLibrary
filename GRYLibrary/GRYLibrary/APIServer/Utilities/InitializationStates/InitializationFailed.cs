namespace GRYLibrary.Core.APIServer.Utilities.InitializationStates
{
    public sealed class InitializationFailed : InitializationState
    {
        public override void Accept(IInitializationStateVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IInitializationStateVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return base.ToString();
        }
    }
}
