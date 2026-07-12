namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    public class HTTP : Protocol
    {
        public const ushort DefaultPort = 80;
        public HTTP() : this(DefaultPort)
        {
        }
        public HTTP(ushort port)
        {
            this.Port = port;
        }

        public override string GetProtocol()
        {
            return "http";
        }
    }
}
