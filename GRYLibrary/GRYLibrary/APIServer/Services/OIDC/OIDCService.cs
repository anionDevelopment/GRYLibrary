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
    /// Supports the authorization-code-flow with PKCE (<see cref="InitiateLoginAsync"/> + <see cref="ExchangeCodeAsync"/>)
    /// and the resource-owner-password-credentials-flow (<see cref="LoginWithPasswordAsync"/>) against any OIDC-compliant provider.
    /// Register as a singleton or scoped service via DI.
    /// </summary>
    public class OIDCService : IOIDCService
    {
        private readonly HttpClient _HttpClient;
        private const string DefaultScope = "openid profile email";

        public OIDCService() : this(new HttpClient()) { }

        public OIDCService(HttpClient httpClient)
        {
            this._HttpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<OIDCAuthorizationRequest> InitiateLoginAsync(OIDCProviderConfiguration provider)
        {
            JsonDocument discovery = await this.FetchDiscoveryAsync(provider);
            string authorizationEndpoint = discovery.RootElement.GetProperty("authorization_endpoint").GetString()!;

            string state = this.GenerateRandomBase64Url(32);
            string codeVerifier = this.GenerateRandomBase64Url(64);
            string codeChallenge = this.ComputeCodeChallenge(codeVerifier);

            string authorizationUrl = authorizationEndpoint
                + "?response_type=code"
                + "&client_id=" + Uri.EscapeDataString(provider.ClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(provider.RedirectUri)
                + "&scope=" + Uri.EscapeDataString(GetScope(provider))
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
            JsonDocument discovery = await this.FetchDiscoveryAsync(provider);
            string tokenEndpoint = discovery.RootElement.GetProperty("token_endpoint").GetString()!;

            Dictionary<string, string> form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = provider.ClientId,
                ["redirect_uri"] = provider.RedirectUri,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
            };
            AddClientSecretIfPresent(form, provider);

            JsonDocument tokenDoc = await this.PostTokenRequestAsync(tokenEndpoint, form);
            string idToken = tokenDoc.RootElement.GetProperty("id_token").GetString()!;
            IDictionary<string, string> claims = await this.ValidateAndParseIdTokenAsync(provider, discovery, idToken);
            return CreateTokenResult(claims);
        }

        /// <inheritdoc/>
        public async Task<OIDCPasswordLoginResult> LoginWithPasswordAsync(OIDCProviderConfiguration provider, string username, string password)
        {
            JsonDocument discovery = await this.FetchDiscoveryAsync(provider);
            string tokenEndpoint = discovery.RootElement.GetProperty("token_endpoint").GetString()!;

            Dictionary<string, string> form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = provider.ClientId,
                ["username"] = username,
                ["password"] = password,
                ["scope"] = GetScope(provider),
            };
            AddClientSecretIfPresent(form, provider);

            JsonDocument tokenDoc = await this.PostTokenRequestAsync(tokenEndpoint, form);
            JsonElement root = tokenDoc.RootElement;

            OIDCPasswordLoginResult result = new OIDCPasswordLoginResult
            {
                AccessToken = root.GetProperty("access_token").GetString()!,
                TokenType = TryGetString(root, "token_type") ?? "Bearer",
                RefreshToken = TryGetString(root, "refresh_token"),
                IdToken = TryGetString(root, "id_token"),
                ExpiresInSeconds = root.TryGetProperty("expires_in", out JsonElement expiresIn) ? expiresIn.GetInt32() : 0,
            };

            if (result.IdToken != null)
            {
                IDictionary<string, string> claims = await this.ValidateAndParseIdTokenAsync(provider, discovery, result.IdToken);
                OIDCTokenResult tokenResult = CreateTokenResult(claims);
                result.Subject = tokenResult.Subject;
                result.PreferredUsername = tokenResult.PreferredUsername;
                result.Email = tokenResult.Email;
                result.Name = tokenResult.Name;
                result.Claims = tokenResult.Claims;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<OIDCTokenResult> ValidateAccessTokenAsync(OIDCProviderConfiguration provider, string accessToken)
        {
            JsonDocument discovery = await this.FetchDiscoveryAsync(provider);
            IDictionary<string, string> claims = await this.ValidateJwtAndParseClaimsAsync(provider, discovery, accessToken, provider.Audience);
            return CreateTokenResult(claims);
        }

        private async Task<JsonDocument> FetchDiscoveryAsync(OIDCProviderConfiguration provider)
        {
            string discoveryUrl = provider.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
            return await this.FetchJsonAsync(discoveryUrl);
        }

        private async Task<JsonDocument> PostTokenRequestAsync(string tokenEndpoint, IDictionary<string, string> form)
        {
            HttpResponseMessage tokenResponse = await this._HttpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
            string tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token endpoint returned {tokenResponse.StatusCode}: {tokenResponseBody}");
            }
            return JsonDocument.Parse(tokenResponseBody);
        }

        private Task<IDictionary<string, string>> ValidateAndParseIdTokenAsync(OIDCProviderConfiguration provider, JsonDocument discovery, string idToken)
        {
            return this.ValidateJwtAndParseClaimsAsync(provider, discovery, idToken, provider.ClientId);
        }

        /// <summary>
        /// Validates a JWT against the provider's issuer, signing-keys and lifetime and returns its claims as a flat dictionary.
        /// The audience is validated only if <paramref name="validAudience"/> is not <see langword="null"/> or empty.
        /// </summary>
        private async Task<IDictionary<string, string>> ValidateJwtAndParseClaimsAsync(OIDCProviderConfiguration provider, JsonDocument discovery, string token, string? validAudience)
        {
            string jwksUri = discovery.RootElement.GetProperty("jwks_uri").GetString()!;
            string issuer = discovery.RootElement.GetProperty("issuer").GetString()!;

            JsonWebKeySet jwks = await this.FetchJwksAsync(jwksUri);

            bool validateAudience = !string.IsNullOrWhiteSpace(validAudience);
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = validAudience,
                IssuerSigningKeys = jwks.Keys,
                ValidateIssuer = true,
                ValidateAudience = validateAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, validationParameters, out SecurityToken _);

            JwtSecurityToken jwt = handler.ReadJwtToken(token);
            return jwt.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.Last().Value);
        }

        private static OIDCTokenResult CreateTokenResult(IDictionary<string, string> claims)
        {
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

        private static string GetScope(OIDCProviderConfiguration provider)
        {
            return string.IsNullOrWhiteSpace(provider.Scope) ? DefaultScope : provider.Scope!;
        }

        private static void AddClientSecretIfPresent(IDictionary<string, string> form, OIDCProviderConfiguration provider)
        {
            if (!string.IsNullOrWhiteSpace(provider.ClientSecret))
            {
                form["client_secret"] = provider.ClientSecret!;
            }
        }

        private static string? TryGetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value) ? value.GetString() : null;
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
