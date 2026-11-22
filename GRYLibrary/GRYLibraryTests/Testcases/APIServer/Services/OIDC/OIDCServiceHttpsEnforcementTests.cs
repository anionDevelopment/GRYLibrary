using GRYLibrary.Core.APIServer.Services.OIDC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.OIDC
{
    [TestClass]
    public class OIDCServiceHttpsEnforcementTests
    {
        private const string HttpsAuthority = "https://login.example.com/realms/myrealm";
        private const string DiscoveryUrl = HttpsAuthority + "/.well-known/openid-configuration";

        /// <summary>Answers requests with predefined bodies and records every requested url, so that no real network-access happens.</summary>
        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly IDictionary<string, string> _ResponseBodies;
            public IList<string> RequestedUrls { get; } = new List<string>();

            public StubHttpMessageHandler(IDictionary<string, string> responseBodies)
            {
                this._ResponseBodies = responseBodies;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                string url = request.RequestUri!.ToString();
                this.RequestedUrls.Add(url);
                if (!this._ResponseBodies.TryGetValue(url, out string? body))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
            }
        }

        private static OIDCProviderConfiguration GetProvider(string authority)
        {
            return new OIDCProviderConfiguration
            {
                Id = "test-provider",
                DisplayName = "Test",
                Authority = authority,
                ClientId = "test-client",
                RedirectUri = "https://app.example.com/oidc-callback",
                Audience = "test-audience",
            };
        }

        private static string GetDiscoveryDocument(string authorizationEndpoint, string tokenEndpoint, string jwksUri)
        {
            return $"{{\"issuer\":\"{HttpsAuthority}\",\"authorization_endpoint\":\"{authorizationEndpoint}\",\"token_endpoint\":\"{tokenEndpoint}\",\"jwks_uri\":\"{jwksUri}\"}}";
        }

        private static (OIDCService service, StubHttpMessageHandler handler) GetServiceWithDiscoveryDocument(string discoveryDocument)
        {
            StubHttpMessageHandler handler = new StubHttpMessageHandler(new Dictionary<string, string> { [DiscoveryUrl] = discoveryDocument });
            return (new OIDCService(new HttpClient(handler)), handler);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [DataRow("http://login.example.com/realms/myrealm")]
        [DataRow("ftp://login.example.com/realms/myrealm")]
        [DataRow("login.example.com/realms/myrealm")]
        [DataRow("")]
        public async Task AuthorityWhichIsNotHttpsIsRejectedBeforeAnyRequest(string authority)
        {
            // arrange
            StubHttpMessageHandler handler = new StubHttpMessageHandler(new Dictionary<string, string>());
            OIDCService service = new OIDCService(new HttpClient(handler));

            // act & assert
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.InitiateLoginAsync(GetProvider(authority)));
            Assert.AreEqual(0, handler.RequestedUrls.Count);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public async Task JwksUriWhichIsNotHttpsIsRejectedBeforeKeysAreFetched()
        {
            // arrange
            const string httpJwksUri = "http://login.example.com/realms/myrealm/certs";
            (OIDCService service, StubHttpMessageHandler handler) = GetServiceWithDiscoveryDocument(GetDiscoveryDocument(HttpsAuthority + "/auth", HttpsAuthority + "/token", httpJwksUri));

            // act & assert
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.ValidateAccessTokenAsync(GetProvider(HttpsAuthority), "irrelevant-token"));
            Assert.IsFalse(handler.RequestedUrls.Contains(httpJwksUri));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public async Task TokenEndpointWhichIsNotHttpsIsRejectedBeforeCodeIsSent()
        {
            // arrange
            const string httpTokenEndpoint = "http://login.example.com/realms/myrealm/token";
            (OIDCService service, StubHttpMessageHandler handler) = GetServiceWithDiscoveryDocument(GetDiscoveryDocument(HttpsAuthority + "/auth", httpTokenEndpoint, HttpsAuthority + "/certs"));

            // act & assert
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.ExchangeCodeAsync(GetProvider(HttpsAuthority), "code", "verifier"));
            Assert.IsFalse(handler.RequestedUrls.Contains(httpTokenEndpoint));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public async Task AuthorizationEndpointWhichIsNotHttpsIsRejected()
        {
            // arrange
            (OIDCService service, StubHttpMessageHandler _) = GetServiceWithDiscoveryDocument(GetDiscoveryDocument("http://login.example.com/realms/myrealm/auth", HttpsAuthority + "/token", HttpsAuthority + "/certs"));

            // act & assert
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.InitiateLoginAsync(GetProvider(HttpsAuthority)));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public async Task HttpsConfigurationIsAccepted()
        {
            // arrange
            const string authorizationEndpoint = HttpsAuthority + "/auth";
            (OIDCService service, StubHttpMessageHandler _) = GetServiceWithDiscoveryDocument(GetDiscoveryDocument(authorizationEndpoint, HttpsAuthority + "/token", HttpsAuthority + "/certs"));

            // act
            OIDCAuthorizationRequest result = await service.InitiateLoginAsync(GetProvider(HttpsAuthority));

            // assert
            Assert.IsTrue(result.AuthorizationUrl.StartsWith(authorizationEndpoint + "?", StringComparison.Ordinal));
        }
    }
}
