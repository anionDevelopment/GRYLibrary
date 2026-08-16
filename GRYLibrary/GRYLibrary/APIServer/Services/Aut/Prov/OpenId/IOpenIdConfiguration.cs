namespace GRYLibrary.Core.APIServer.Services.Aut.Prov.OpenId
{
    public interface IOpenIdConfiguration : IAuthenticationProviderConfiguration
    {
        public string URL { get; set; }
        public string Label { get; set; }
        public string ClientUsername { get; set; }
        public string ClientPassword { get; set; }
    }
}
