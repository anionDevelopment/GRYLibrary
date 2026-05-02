namespace GRYLibrary.Core.APIServer.Services.Aut.Prov.OpenId
{
    public class OpenIdConfiguration : IOpenIdConfiguration
    {
        public string ProviderIdentifier { get; set; }
        public string URL { get; set; }
        public string Label { get; set; }
        public string ClientUsername { get; set; }
        public string ClientPassword { get; set; }
        public bool Enabled { get; set; }

        public IAuthenticationProvider CreateProvider()
        {
            return new OpenIdProvider(this);
        }
    }
}
