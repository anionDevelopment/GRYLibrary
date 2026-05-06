using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using Microsoft.Extensions.Logging;
using System;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    public interface IGRYLog : IDisposable, ILogger, IGeneralLogger
    {

        public IGRYLogConfiguration Configuration { get; set; }
        public string? BasePath { get; set; }
        public void LogProgramOutput(string message, string[] stdOutLines, string[] stdErrLines, LogLevel logevel);
        public IDisposable UseSubNamespace(string loggerName);
        public FixedSizeQueue<LogItem> LastLogEntries { get; }
        public uint GetAmountOfErrors();
        public uint GetAmountOfWarnings();
        public void Log(Exception exception);
        public void Log(string message);
        public void Log(string message, LogLevel logLevel);

        public void Log(string message, Exception exception);
        public void Log(string message, Exception exception, LogLevel logLevel);

        public void Log(Func<string> message, LogLevel logLevel);

        public void Log(Func<string> message, Exception exception);
        public void Log(Func<string> getMessageFunction, Exception? exception, LogLevel logLevel);
        public void Log(LogItem logitem);
        public void ApplyConfiguration(IGRYLogConfiguration configuration);
        public void RunTask(Action updatePreviewsInRuntimeData, string taskName, bool catchErrors, LogLevel logLevelForOverhead);
    }
}
