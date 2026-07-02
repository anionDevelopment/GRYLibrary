using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.APIServer.Services.Logger
{
    public interface IAuditLog
    {
        public IGRYLog Logger { get; }
    }
    public class AuditLog : SemanticLogger, IAuditLog
    {
        public IGRYLog Logger => this.Log;
        public AuditLog(IGRYLogConfiguration config, string? basePath) : base(config, "Audit", basePath)
        {
        }
    }
}
