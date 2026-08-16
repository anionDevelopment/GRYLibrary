using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;

namespace GRYLibrary.Core.APIServer.Utilities
{

    public class GetLoggerVisitor : IExecutionModeVisitor<IGRYLog>
    {
        private readonly IGRYLogConfiguration _LogConfiguration;
        private readonly string _BaseFolder;
        private readonly string _LoggerName;
        private readonly IGRYLog _InitialLog;
        private readonly bool _Verbose;
        public GetLoggerVisitor(IGRYLogConfiguration logConfiguration, string baseFolder, string loggerName, IGRYLog initialLog, bool verbose)
        {
            this._LogConfiguration = logConfiguration;
            this._BaseFolder = baseFolder;
            this._LoggerName = loggerName;
            this._InitialLog = initialLog;
            this._Verbose = verbose;
        }
        public IGRYLog Handle(Analysis analysis)
        {
            return GRYLog.Create(this._LoggerName, this._Verbose);
        }

        public IGRYLog Handle(RunProgram runProgram)
        {
            return this.GetUsualLog();
        }

        public IGRYLog Handle(TestRun testRun)
        {
            return this.GetUsualLog();
        }

        private IGRYLog GetUsualLog()
        {
            IGRYLog result = GeneralLogger.CreateUsingGRYLog(this._LogConfiguration, this._BaseFolder, this._InitialLog);
            result.BasePath = this._BaseFolder;
            result.UseSubNamespace(this._LoggerName);
            return result;
        }
    }
}
