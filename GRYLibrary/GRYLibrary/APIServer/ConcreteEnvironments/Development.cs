namespace GRYLibrary.Core.APIServer.ConcreteEnvironments
{
    public class Development : GRYEnvironment
    {
        public static Development Instance { get; } = new Development();
        private Development()
        {
        }
        public override void Accept(IEnvironmentVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IEnvironmentVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}