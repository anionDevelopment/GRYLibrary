using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.Services.OIDC
{
    /// <summary>
    /// Default implementation of <see cref="IOIDCService"/>.
    /// Performs PKCE-based Authorization Code flow against any OIDC-compliant provider.
    /// Register as a singleton or scoped service via DI.
    /// </summary>
    public class OIDCService : IOIDCService
    {
        private readonly HttpClient _HttpClient;

        public OIDCService() : this(new HttpClient()) { }

        public OIDCService(HttpClient httpClient)
        {
            this._HttpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<OIDCAuthorizationRequest> InitiateLoginAsync(OIDCProviderConfiguration provider)
        {
            string discoveryUrl = provider.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
            JsonDocument discovery = await this.FetchJsonAsync(discoveryUrl);
            string authorizationEndpoint = discovery.RootElement.GetProperty("authorization_endpoint").GetString()!;

            string state = this.GenerateRandomBase64Url(32);
            string codeVerifier = this.GenerateRandomBase64Url(64);
            string codeChallenge = this.ComputeCodeChallenge(codeVerifier);

            string authorizationUrl = authorizationEndpoint
                + "?response_type=code"
                + "&client_id=" + Uri.EscapeDataString(provider.ClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(provider.RedirectUri)
                + "&scope=" + Uri.EscapeDataString("openid profile email")
                + "&state=" + Uri.EscapeDataString(state)
                + "&code_challenge=" + Uri.EscapeDataString(codeChallenge)
                + "&code_challenge_method=S256";

            return new OIDCAuthorizationRequest
            {
                AuthorizationUrl = authorizationUrl,
                State = state,
                CodeVerifier = codeVerifier,
            };
        }

        /// <inheritdoc/>
        public async Task<OIDCTokenResult> ExchangeCodeAsync(OIDCProviderConfiguration provider, string code, string codeVerifier)
        {
            string discoveryUrl = provider.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
            JsonDocument discovery = await this.FetchJsonAsync(discoveryUrl);
            string tokenEndpoint = discovery.RootElement.GetProperty("token_endpoint").GetString()!;
            string jwksUri = discovery.RootElement.GetProperty("jwks_uri").GetString()!;
            string issuer = discovery.RootElement.GetProperty("issuer").GetString()!;

            FormUrlEncodedContent tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = provider.ClientId,
                ["redirect_uri"] = provider.RedirectUri,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
            });

            HttpResponseMessage tokenResponse = await this._HttpClient.PostAsync(tokenEndpoint, tokenRequest);
            string tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token endpoint returned {tokenResponse.StatusCode}: {tokenResponseBody}");
            }

            JsonDocument tokenDoc = JsonDocument.Parse(tokenResponseBody);
            string idToken = tokenDoc.RootElement.GetProperty("id_token").GetString()!;

            JsonWebKeySet jwks = await this.FetchJwksAsync(jwksUri);

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = provider.ClientId,
                IssuerSigningKeys = jwks.Keys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(idToken, validationParameters, out SecurityToken _);

            JwtSecurityToken jwt = handler.ReadJwtToken(idToken);

            IDictionary<string, string> claims = jwt.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.Last().Value);

            string subject = claims.TryGetValue("sub", out string? sub) ? sub! : throw new InvalidOperationException("ID token missing 'sub' claim.");
            claims.TryGetValue("preferred_username", out string? preferredUsername);
            claims.TryGetValue("email", out string? email);
            claims.TryGetValue("name", out string? name);

            return new OIDCTokenResult
            {
                Subject = subject,
                PreferredUsername = preferredUsername,
                Email = email,
                Name = name,
                Claims = claims,
            };
        }

        private async Task<JsonDocument> FetchJsonAsync(string url)
        {
            HttpResponseMessage response = await this._HttpClient.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch '{url}': {response.StatusCode} {body}");
            }
            return JsonDocument.Parse(body);
        }

        private async Task<JsonWebKeySet> FetchJwksAsync(string jwksUri)
        {
            HttpResponseMessage response = await this._HttpClient.GetAsync(jwksUri);
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch JWKS from '{jwksUri}': {response.StatusCode}");
            }
            return new JsonWebKeySet(body);
        }

        private string GenerateRandomBase64Url(int byteLength)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Base64UrlEncode(bytes);
        }

        private string ComputeCodeChallenge(string codeVerifier)
        {
            byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
