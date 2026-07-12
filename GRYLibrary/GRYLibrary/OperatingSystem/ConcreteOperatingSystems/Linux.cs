namespace GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems
{
    public class Linux : OperatingSystem
    {
        private Linux() { }
        public static OperatingSystem Instance { get; } = new Linux();
        public override void Accept(IOperatingSystemVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IOperatingSystemVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}