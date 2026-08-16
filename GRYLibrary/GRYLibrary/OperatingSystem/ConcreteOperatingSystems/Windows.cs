namespace GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems
{
    public class Windows : OperatingSystem
    {
        private Windows() { }
        public static OperatingSystem Instance { get; } = new Windows();

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