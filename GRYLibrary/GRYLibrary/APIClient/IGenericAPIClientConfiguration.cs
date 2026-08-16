using System.Net.Http;

namespace GRYLibrary.Core.APIClient
{
    public interface IGenericAPIClientConfiguration
    {
        public string APIKey { get; set; }
        public string APIAddress { get; set; }
        public string TestRoute { get; set; }
        /// <remarks>
        /// See https://stackoverflow.com/a/30605900/3905529 for hints about the proxy-usage.
        /// </remarks>
        public HttpClientHandler HttpClientHandler { get; set; }
        public bool Verbose { get; set; }
    }
}
