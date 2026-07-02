using GRYLibrary.Core.APIServer.Services;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIClient
{
    public interface IGenericAPIClient : IExternalService
    {
        public IGenericAPIClientConfiguration Configuration { get; set; }
        public Task<decimal> GetAsDecimalAsync(string route);
        public Task<string> GetAsStringAsync(string route);
        Task PostAsync(string route, string body);
        Task PutAsync(string route, string body);
    }
}
