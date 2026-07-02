using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.APIServer.Services.Logger
{
    public interface IRequestsLog
    {
        public IGRYLog Logger { get; }
    }
    public class RequestsLog : SemanticLogger, IRequestsLog
    {
        public IGRYLog Logger => this.Log;
        public RequestsLog(IGRYLogConfiguration config, string? basePath) : base(config, "Requests", basePath)
        {
        }
    }
}
