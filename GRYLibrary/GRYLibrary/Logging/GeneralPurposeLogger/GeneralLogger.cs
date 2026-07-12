using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace GRYLibrary.Core.Logging.GeneralPurposeLogger
{
    public class GeneralLogger : IGeneralLogger
    {
        public Action<LogItem> AddLogEntry { get; set; }
        internal ITimeService _TimeService = new TimeService();
        private static uint _LoggerCounter = 0;
        private string _LoggerId;
        public GeneralLogger()
        {
            _LoggerCounter = _LoggerCounter + 1;
            this._LoggerId = this.GetType().Name + _LoggerCounter.ToString();
        }
        public static GeneralLogger NoLog()
        {
            return new GeneralLogger() { AddLogEntry = (logItem) => { } };
        }

        public static IGRYLog CreateUsingGRYLog(IGRYLogConfiguration configuration)
        {
            return CreateUsingGRYLog(configuration, Directory.GetCurrentDirectory());
        }

        public static IGRYLog CreateUsingGRYLog(IGRYLogConfiguration configuration, string basePath = null, string? name = null)
        {
            return CreateUsingGRYLog(configuration, basePath, GRYLog.Create(configuration, name), name);
        }
        public static IGRYLog CreateUsingGRYLog(IGRYLogConfiguration configuration, string basePath, IGRYLog logToUse, string? name = null)
        {
            IGRYLog logObject = logToUse;
            logObject.ApplyConfiguration(configuration);
            logObject.BasePath = basePath;
            return logObject;
        }

        public static IGRYLog NoLogAsGRYLog()
        {
            GRYLog logObject = GRYLog.Create();
            logObject.Configuration.LogTargets.Clear();
            return logObject;
        }


        public static IGRYLog CreateUsingConsole()
        {
            return CreateUsingConsole(LogLevel.Information);
        }
        public static IGRYLog CreateUsingConsole(LogLevel logLevel)
        {
            GRYLog logObject = GRYLog.Create();
            logObject.Configuration.LogTargets = [.. logObject.Configuration.LogTargets.Where(target => target is GRYLogger.ConcreteLogTargets.Console)];
            foreach (GRYLogTarget target in logObject.Configuration.LogTargets)
            {
                target.Format = GRYLogLogFormat.GRYLogFormat;
                target.SetLogLevel(logLevel);
            }
            return logObject;
        }

        public void Log(Exception exception)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), exception));
        }

        private DateTimeOffset GetTime()
        {
            return this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
        }

        public void Log(string message)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message));
        }

        public void Log(string message, LogLevel logLevel)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message, logLevel));
        }

        public void Log(string message, Exception exception)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message, exception));
        }

        public void Log(string message, Exception exception, LogLevel logLevel)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message, exception, logLevel));
        }

        public void Log(Func<string> message, LogLevel logLevel)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message, logLevel));
        }

        public void Log(Func<string> message, Exception exception)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), message, exception));
        }

        public void Log(Func<string> getMessageFunction, Exception? exception, LogLevel logLevel)
        {
            this.AddLogEntry(new LogItem(this.GetTime(), getMessageFunction, exception, logLevel));
        }

        public void Log(LogItem logitem)
        {
            this.AddLogEntry(logitem);
        }
        public string GetLoggerId()
        {
            return this._LoggerId;
        }
    }
}