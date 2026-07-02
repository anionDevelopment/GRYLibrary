using Microsoft.AspNetCore.Http;
using System;

namespace GRYLibrary.Core.APIServer.Services.Aut.Prov.H
{
    public class HeaderProvider : CredentialsProviderBase, IHeaderProvider
    {
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
