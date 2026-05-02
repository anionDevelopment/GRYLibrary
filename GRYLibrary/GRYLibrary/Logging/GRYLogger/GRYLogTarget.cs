using GRYLibrary.Core.AOA;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    [XmlInclude(typeof(ConcreteLogTargets.Console))]
    [XmlInclude(typeof(LogFile))]
    [XmlInclude(typeof(Observer))]
    [XmlInclude(typeof(WindowsEventLog))]
    public abstract class GRYLogTarget : IDisposable
    {

        public GRYLogLogFormat Format { get; set; } = GRYLogLogFormat.GRYLogFormat;

        public HashSet<LogLevel> LogLevels { get; set; } =
            [
                 LogLevel.Information,
                 LogLevel.Warning,
                 LogLevel.Error,
                 LogLevel.Critical
            ];
        public bool Enabled { get; set; } = true;
        public abstract HashSet<Type> FurtherGetExtraTypesWhichAreRequiredForSerialization();
        internal void Execute(LogItem logItem, GRYLog logObject)
        {
            this.ExecuteImplementation(logItem, logObject);
        }

        protected abstract void ExecuteImplementation(LogItem logItem, GRYLog logObject);
        #region Overhead
        public override bool Equals(object @object)
        {
            return @object is not null && @object.GetType().Equals(this.GetType());
        }

        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }

        public override string ToString()
        {
            return Generic.GenericToString(this);
        }

        public abstract void Dispose();

        internal static ISet<GRYLogTarget> GetAll()
        {
            HashSet<GRYLogTarget> result =
            [
                new ConcreteLogTargets.Console(),
                new LogFile(),
                new Observer(),
                new Syslog()
            ];
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result.Add(new WindowsEventLog());
            }
            return result;
        }

        internal void SetLogLevel(LogLevel logLevel)
        {
            this.LogLevels.Clear();
            foreach (LogLevel level in GetAllLogLevel())
            {
                if (level >= logLevel)
                {
                    this.LogLevels.Add(level);
                }
            }
        }
        public static ISet<LogLevel> GetAllLogLevel()
        {
            ISet<LogLevel> result = new HashSet<LogLevel>() {
                LogLevel.Trace,
                LogLevel.Debug,
                LogLevel.Information,
                LogLevel.Warning,
                LogLevel.Error,
                LogLevel.Critical,
            };
            return result;
        }
        #endregion
    }
}