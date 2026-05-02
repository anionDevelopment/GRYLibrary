using GRYLibrary.Core.APIServer.Services;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public interface IReconnectableDatabase : IExternalService
    {
        public void SetLogConnectionAttemptErrors(bool enabled);
    }
}
