using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;
using GRYLibrary.Core.Misc.FilePath;

namespace GRYLibrary.Core.APIServer.Services.Logger
{
    public interface IInitialLog
    {
        public IGRYLog Logger { get; }
    }
    public class InitialLog : SemanticLogger, IInitialLog
    {
        public IGRYLog Logger => this.Log;
        public InitialLog() : this(GRYLogConfiguration.GetCommonConfiguration((AbstractFilePath?)null, false))
        {
        }

        public InitialLog(IGRYLogConfiguration config) : base(config, "Initial", null)
        {
            foreach (var target in this.LogConfiguration.LogTargets)
            {
                if (target is LogFile)
                {
                    target.Enabled = false;
                }
            }
        }
    }
}
