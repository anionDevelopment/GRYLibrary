using GRYLibrary.Core.Misc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets
{
    [SupportedOSPlatform("windows")]
    public sealed class WindowsEventLog : GRYLogTarget
    {
        public WindowsEventLog() { }
        protected override void ExecuteImplementation(LogItem logItem, GRYLog logObject)
        {
            using EventLog eventLog = new(Utilities.GetNameOfCurrentExecutable()) { Source = logObject.Configuration.Name };
            string messageId = string.Empty;
            eventLog.WriteEntry(messageId + logItem.PlainMessage, ConvertLogLevel(logItem.LogLevel), logItem.EventId, logItem.Category);
        }

        private static EventLogEntryType ConvertLogLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Trace)
            {
                return EventLogEntryType.Information;
            }
            if (logLevel == LogLevel.Debug)
            {
                return EventLogEntryType.Information;
            }
            if (logLevel == LogLevel.Information)
            {
                return EventLogEntryType.Information;
            }
            if (logLevel == LogLevel.Warning)
            {
                return EventLogEntryType.Warning;
            }
            if (logLevel == LogLevel.Error)
            {
                return EventLogEntryType.Error;
            }
            if (logLevel == LogLevel.Critical)
            {
                return EventLogEntryType.Error;
            }
            throw new KeyNotFoundException($"Loglevel '{logLevel}' is not writeable to windows-eventlog");
        }
        public override HashSet<Type> FurtherGetExtraTypesWhichAreRequiredForSerialization()
        {
            return [];
        }

        public override void Dispose()
        {
            Utilities.NoOperation();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}