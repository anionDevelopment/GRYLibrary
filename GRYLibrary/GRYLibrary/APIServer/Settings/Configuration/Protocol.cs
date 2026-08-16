using System.Xml.Serialization;

namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    [XmlInclude(typeof(HTTP))]
    [XmlInclude(typeof(HTTPS))]
    public abstract class Protocol
    {
        public ushort Port { get; set; }
        public abstract string GetProtocol();
    }
}
