namespace GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats
{
    public interface IConfigurationFormat
    {
        public void Accept(IConfigurationFormatVisitor visitor);
        public T Accept<T>(IConfigurationFormatVisitor<T> visitor);
    }
    public interface IConfigurationFormatVisitor
    {
        void Handle(JSON jSON);
        void Handle(XML xML);
    }
    public interface IConfigurationFormatVisitor<T>
    {
        T Handle(XML xML);
        T Handle(JSON jSON);
    }
}