namespace GRYLibrary.Core.APIServer.Services.Aut.Prov
{
    public interface IAuthenticationProviderConfiguration
    {
        public string ProviderIdentifier { get; set; }
        public bool Enabled { get; set; }

        public IAuthenticationProvider CreateProvider();
    }
}
