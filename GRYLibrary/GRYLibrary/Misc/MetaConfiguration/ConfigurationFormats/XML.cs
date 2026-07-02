namespace GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats
{
    public class XML : IConfigurationFormat
    {
        public static XML Instance { get; } = new XML();
        private XML() { }
        public void Accept(IConfigurationFormatVisitor visitor)
        {
            visitor.Handle(this);
        }

        public T Accept<T>(IConfigurationFormatVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}