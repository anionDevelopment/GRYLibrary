using Microsoft.AspNetCore.Http;

namespace GRYLibrary.Core.APIServer.Services
{
    public abstract class CredentialsProviderBase
    {

        public abstract bool ContainsCredentials(HttpContext context);

        public abstract string ExtractSecret(HttpContext context);
        public bool TryGetAuthentication(HttpContext context, out string? accessToken)
        {
            try
            {
                if (this.ContainsCredentials(context))
                {
                    string secret = this.ExtractSecret(context);
                    if (!string.IsNullOrEmpty(secret))
                    {
                        accessToken = secret;
                        return true;
                    }
                }
            }
            catch
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            }
            accessToken = null;
            return false;
        }
    }
}
