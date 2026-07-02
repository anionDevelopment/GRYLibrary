using GRYLibrary.Core.Logging.GRYLogger;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Logger
{
    public abstract class SemanticLogger
    {
        protected IGRYLogConfiguration LogConfiguration { get; private set; }
        protected IGRYLog Log { get; private set; }
        private readonly IList<SemanticLogger> _AllSemanticLoggers = new List<SemanticLogger>();
        public IList<SemanticLogger> AllSemanticLogger
        {
            get { return [.. this._AllSemanticLoggers]; }
        }
        protected SemanticLogger(IGRYLogConfiguration config, string loggerName, string? basePath)
        {
            this.LogConfiguration = config;
            this.Log = GRYLog.Create(config, loggerName, true);
            this.Log.BasePath = basePath;
            this._AllSemanticLoggers.Add(this);
        }
    }
}
