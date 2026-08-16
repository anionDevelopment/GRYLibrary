namespace GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems
{
    public class OSX : OperatingSystem
    {
        private OSX() { }
        public static OperatingSystem Instance { get; } = new OSX();
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