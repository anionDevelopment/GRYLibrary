namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    public class HTTPS : Protocol
    {
        public const ushort DefaultPort = 443;
        public TLSCertificateInformation TLSCertificateInformation { get; set; }
        public HTTPS() : this(DefaultPort, default)
        {
        }
        public HTTPS(TLSCertificateInformation tlsCertificateInformation) : this(DefaultPort, tlsCertificateInformation)
        {
        }
        public HTTPS(ushort port, TLSCertificateInformation tlsCertificateInformation)
        {
            this.Port = port;
            this.TLSCertificateInformation = tlsCertificateInformation;
        }
        public override string GetProtocol()
        {
            return "https";
        }
    }
}
