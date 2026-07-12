using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    public struct LogItem
    {
        private string _FormattedMessage;
        private int _ColorBegin;
        private int _ColorEnd;
        private bool _MessageLoaded;
        private bool _FormatingLoaded;
        private ConsoleColor _ConsoleColor;
        private string _PlainMessage;
        private readonly Func<string> _GetMessageFunction;
        internal ITimeService _TimeService = new TimeService();
        internal bool TimestampInUTC = false;
        public DateTimeOffset Moment { get; set; }
        /**
         * Only relevant for  <see cref="ConcreteLogTargets.WindowsEventLog"/>.
         */
        public int EventId { get; set; }
        /**
         * Only relevant for  <see cref="ConcreteLogTargets.WindowsEventLog"/>.
         */
        public short Category { get; set; }

        public LogLevel LogLevel { get; internal set; }
        public Exception Exception { get; }
        public ISet<GRYLogTarget> LogTargets { get; set; } = GRYLogTarget.GetAll();

        public string PlainMessage
        {
            get
            {
                if (!this._MessageLoaded)
                {
                    string plainMessage = this._GetMessageFunction();
                    if (this.Exception != null)
                    {
                        plainMessage = GUtilities.GetExceptionMessage(this.Exception, plainMessage);
                    }
                    this._PlainMessage = plainMessage;
                    this._MessageLoaded = true;
                }

                return this._PlainMessage;
            }
        }

        private readonly GRYLogConfiguration DefaultConfigurationGRYLogConfiguration = new GRYLogConfiguration();

        #region Constructors
        public LogItem(DateTimeOffset moment, Exception exception) : this(moment, () => "An error occurred.", exception, LogLevel.Error)
        {
        }
        public LogItem(DateTimeOffset moment, string message) : this(moment, () => message, null, LogLevel.Information)
        {
        }
        public LogItem(DateTimeOffset moment, string message, LogLevel logLevel) : this(moment, () => message, null, logLevel)
        {
        }

        public LogItem(DateTimeOffset moment, string message, Exception exception) : this(moment, () => message, exception, LogLevel.Error)
        {
        }
        public LogItem(DateTimeOffset moment, string message, Exception exception, LogLevel logLevel) : this(moment, () => message, exception, logLevel)
        {
        }

        public LogItem(DateTimeOffset moment, Func<string> message, LogLevel logLevel) : this(moment, message, null, logLevel)
        {
        }

        public LogItem(DateTimeOffset moment, Func<string> message, Exception exception) : this(moment, message, exception, LogLevel.Error)
        {
        }
        public LogItem(DateTimeOffset moment, Func<string> getMessageFunction, Exception? exception, LogLevel logLevel) : this()
        {
            this._GetMessageFunction = getMessageFunction;
            this._MessageLoaded = false;
            this._FormatingLoaded = false;
            this.LogLevel = logLevel;
            this.Exception = exception;
            this.EventId = 101;
            this.Category = 1;
            this.Moment = moment;
            this.DefaultConfigurationGRYLogConfiguration.Initliaze();
        }
        #endregion 
        public void Format(IGRYLogConfiguration configuration, out string formattedMessage, out int colorBegin, out int colorEnd, out ConsoleColor consoleColor, GRYLogLogFormat format)
        {
            if (!this._FormatingLoaded)
            {
                this.FormatMessage(configuration, this.PlainMessage, this.Moment, this.LogLevel, format, out string fm, out int cb, out int ce, out ConsoleColor cc);
                this._FormattedMessage = fm;
                this._ColorBegin = cb;
                this._ColorEnd = ce;
                this._ConsoleColor = cc;
                this._FormatingLoaded = true;
            }
            formattedMessage = this._FormattedMessage;
            colorBegin = this._ColorBegin;
            colorEnd = this._ColorEnd;
            consoleColor = this._ConsoleColor;
        }
        public readonly bool IsErrorEntry()
        {
            return this.LogLevel is LogLevel.Critical or LogLevel.Error;
        }

        private readonly void FormatMessage(IGRYLogConfiguration configuration, string message, DateTimeOffset momentOfLogEntry, LogLevel loglevel, GRYLogLogFormat format, out string formattedMessage, out int colorBegin, out int colorEnd, out ConsoleColor consoleColor)
        {
            consoleColor = configuration.GetLoggedMessageTypesConfigurationByLogLevel(loglevel).ConsoleColor;
            if (!string.IsNullOrEmpty(configuration.Name))
            {
                message = $"[{configuration.Name.Trim()}] {message}";
            }
            switch (format)
            {
                case GRYLogLogFormat.OnlyMessage:
                    formattedMessage = message;
                    colorBegin = 0;
                    colorEnd = 0;
                    break;
                case GRYLogLogFormat.GRYLogFormat:
                    string part1 = $"[{momentOfLogEntry.ToString(configuration.DateFormat)}] [";
                    string part2 = configuration.GetLoggedMessageTypesConfigurationByLogLevel(loglevel).CustomText;
                    string part3 = "] " + message;
                    formattedMessage = part1 + part2 + part3;
                    colorBegin = part1.Length;
                    colorEnd = part1.Length + part2.Length;
                    break;
                case GRYLogLogFormat.DateOnly:
                    formattedMessage = momentOfLogEntry.ToString(configuration.DateFormat) + " " + message;
                    colorBegin = 0;
                    colorEnd = 0;
                    break;
                default:
                    throw new KeyNotFoundException($"Formatting {nameof(GRYLogLogFormat)} '{loglevel}' is not implemented yet.");
            }
        }
        public override string ToString()
        {
            this.FormatMessage(this.DefaultConfigurationGRYLogConfiguration, this.PlainMessage, this.Moment, this.LogLevel, GRYLogLogFormat.GRYLogFormat, out string fm, out int cb, out int ce, out ConsoleColor cc);
            return fm;
        }
        public override readonly bool Equals(object obj)
        {
            return obj is LogItem item &&
                   this._PlainMessage == item._PlainMessage &&
                   this.EventId == item.EventId &&
                   this.Category == item.Category &&
                   this.LogLevel == item.LogLevel;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(this._PlainMessage, this.EventId, this.Category, this.LogLevel);
        }

        public static bool operator ==(LogItem left, LogItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LogItem left, LogItem right)
        {
            return !(left == right);
        }

    }
}