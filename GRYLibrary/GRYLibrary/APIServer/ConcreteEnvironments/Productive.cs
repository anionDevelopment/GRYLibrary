namespace GRYLibrary.Core.APIServer.ConcreteEnvironments
{
    public class Productive : GRYEnvironment
    {
        public static Productive Instance { get; } = new Productive();
        private Productive()
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