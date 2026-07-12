namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    public interface IServerConfiguration
    {
        public string Domain { get; set; }
        public string PublicUrl { get; set; }
        public Protocol Protocol { get; set; }
        public string DevelopmentCertificatePasswordHex { get; set; }
        public string DevelopmentCertificatePFXHex { get; set; }
        public bool HostAPISpecificationForInNonDevelopmentEnvironment { get; set; }
        public bool TrustForwardedHeader { get; set; }
        public void SetDomainAndPublichUrlToDefault(string domain);
        public string GetServerAddress();
    }
    public class ServerConfiguration : IServerConfiguration
    {
        public const string APIRoutePrefix = "/API";
        public const string APISpecificationDocumentName = "APISpecification";
        public const string ResourcesSubPath = $"Other/Resources";
        public const string APIResourcesSubPath = $"{APIRoutePrefix}/{ResourcesSubPath}";
        public const string TermsOfServiceURLSubPath = $"{APIResourcesSubPath}/Information/TermsOfService";
        public const string ContactURLSubPath = $"{APIResourcesSubPath}/Information/Contact";
        public const string LicenseURLSubPath = $"{APIResourcesSubPath}/Information/License";
        public string Domain { get; set; }
        public string PublicUrl { get; set; }
        public void SetDomainAndPublichUrlToDefault(string domain)
        {
            this.Domain = domain;
            this.PublicUrl = $"{this.Protocol.GetProtocol()}://{domain}:{this.Protocol.Port}";
        }
        public Protocol Protocol { get; set; } = new HTTPS();
        public string DevelopmentCertificatePasswordHex { get; set; }
        public string DevelopmentCertificatePFXHex { get; set; }
        public bool HostAPISpecificationForInNonDevelopmentEnvironment { get; set; } = false;
        public bool TrustForwardedHeader { get; set; } = false;
        public string GetServerAddress()
        {
            if (this.PublicUrl == null)
            {
                return $"{this.Protocol.GetProtocol()}://{this.Domain}:{this.Protocol.Port}";
            }
            else
            {
                return this.PublicUrl;
            }
        }
    }
}
