using GRYLibrary.Core.Misc;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIClient
{
    public class GenericAPIClient : IGenericAPIClient
    {
        public IGenericAPIClientConfiguration Configuration { get; set; }
        public GenericAPIClient(IGenericAPIClientConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        protected HttpClient GetHTTPClient()
        {
            HttpClient httpClient;
            if (this.Configuration.HttpClientHandler == null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                httpClient = new HttpClient(handler: this.Configuration.HttpClientHandler);
            }
            return httpClient;
        }
        public async Task<decimal> GetAsDecimalAsync(string route)
        {
            return decimal.Parse(await this.GetAsStringAsync(route), CultureInfo.InvariantCulture);
        }

        public async Task<string> GetAsStringAsync(string route)
        {
            return await this.SendAsStringAsync(route, HttpMethod.Get);
        }

        public async Task PostAsync(string route, string body)
        {
            await this.GetResponse(route, HttpMethod.Post, body);
        }

        public async Task PutAsync(string route, string body)
        {
            await this.GetResponse(route, HttpMethod.Put, body);
        }

        private async Task<string> SendAsStringAsync(string route, HttpMethod method)
        {
            HttpResponseMessage response = await this.GetResponse(route, method, null);
            return await response.Content.ReadAsStringAsync();
        }

        public (bool, Exception?) IsAvailable()
        {
            if (this.Configuration.TestRoute == null)
            {
                throw new NotSupportedException($"{nameof(IsAvailable)} is not supported for this API-client.");
            }
            else
            {
                try
                {
                    this.GetResponse(this.Configuration.TestRoute, HttpMethod.Get, null).WaitAndGetResult();
                    return (true, null);
                }
                catch (Exception e)
                {
                    return (false, e);
                }
            }
        }

        private async Task<HttpResponseMessage> GetResponse(string route, HttpMethod method, string body)
        {
            using HttpClient client = this.GetHTTPClient();
            using HttpRequestMessage request = new HttpRequestMessage(method, $"{this.Configuration.APIAddress}/{route}");

            if (body != null)
            {
                request.Content = new StringContent(body);
            }
            if (this.Configuration.APIKey != null)
            {
                request.Headers.Add("APIKey", this.Configuration.APIKey);
            }
            HttpResponseMessage result = await client.SendAsync(request);
            CheckResponse(request, result, this.Configuration.Verbose);
            return result;
        }

        internal static void CheckResponse(HttpRequestMessage request, HttpResponseMessage response, bool verbose)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                throw new HttpRequestException($"Request resulted in response-statuscode {(int)response.StatusCode}. The request was {RequestToString(request, verbose)}.");
            }
        }

        internal static string RequestToString(HttpRequestMessage request, bool verbose)
        {
            string result = $"<{request.Method} {request.RequestUri}>";
            if (verbose)
            {
                if (request.Content != null)
                {
                    string requestBody = request.Content.ReadAsStringAsync().Result;
                    if (requestBody != null)
                    {
                        result = $"{result} (Body: \"{requestBody}\")";
                    }
                }
            }
            return result;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
