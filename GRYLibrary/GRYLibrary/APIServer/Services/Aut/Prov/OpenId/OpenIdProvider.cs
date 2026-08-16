using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.Aut.Prov.OpenId
{
    public class OpenIdProvider : CredentialsProviderBase, IOpenIdProvider
    {
        private readonly IOpenIdConfiguration _Configuration;
        public OpenIdProvider(IOpenIdConfiguration configuration)
        {
            this._Configuration = configuration;
        }

        public override bool ContainsCredentials(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override string ExtractSecret(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsApplicable(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
