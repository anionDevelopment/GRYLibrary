namespace GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats
{
    public class JSON : IConfigurationFormat
    {
        public static JSON Instance { get; } = new JSON();
        private JSON() { }
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