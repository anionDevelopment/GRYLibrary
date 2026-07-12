using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.Extensions.Logging;
using System;

namespace GRYLibrary.Core.Logging.GeneralPurposeLogger
{
    public interface IGeneralLogger
    {
        public Action<LogItem> AddLogEntry { get; set; }
        public void Log(Exception exception);
        public void Log(string message);
        public void Log(string message, LogLevel logLevel);

        public void Log(string message, Exception exception);
        public void Log(string message, Exception exception, LogLevel logLevel);

        public void Log(Func<string> message, LogLevel logLevel);

        public void Log(Func<string> message, Exception exception);
        public void Log(Func<string> getMessageFunction, Exception? exception, LogLevel logLevel);
        public void Log(LogItem logitem);
        public string GetLoggerId();
    }
}