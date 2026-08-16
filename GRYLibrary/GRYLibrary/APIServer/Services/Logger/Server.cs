using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;
using GRYLibrary.Core.Misc.FilePath;

namespace GRYLibrary.Core.APIServer.Services.Logger
{
    public interface IServerLog
    {
        public IGRYLog Logger { get; }
    }
    public class ServerLog : SemanticLogger, IServerLog
    {
        public IGRYLog Logger => this.Log;
        public ServerLog(IGRYLogConfiguration config, string? basePath) : base(config, "Server", basePath)
        {
        }

        public static IServerLog GetTransientLog()
        {
            ServerLog result = new ServerLog(GRYLogConfiguration.GetCommonConfiguration((AbstractFilePath?)null, false), null);
            foreach (var target in result.Logger.Configuration.LogTargets)
            {
                if (target is LogFile)
                {
                    target.Enabled = false;
                }
            }
            return result;
        }
    }
}
