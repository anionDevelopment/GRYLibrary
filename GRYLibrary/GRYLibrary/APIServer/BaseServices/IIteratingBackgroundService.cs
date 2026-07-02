using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.BaseServices
{
    public interface IIteratingBackgroundService
    {
        public void StartAsync();
        public Task Stop();
    }
}
