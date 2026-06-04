using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;
using GRYLibrary.Core.Misc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    public sealed class GRYLog : IGRYLog
    {
        public IGRYLogConfiguration Configuration { get; set; }
        /// <summary>
        /// Represents the basepath for the possibly relative path when accessing <see cref="LogFile.File"/>.
        /// </summary>
        public string BasePath { get; set; }
        private readonly static object _LockObject = new();
        private readonly bool _Initialized = false;
        private uint _AmountOfErrors = 0;
        private uint _AmountOfWarnings = 0;
        public ITimeService _TimeService = new TimeService();
        internal readonly ConsoleColor _ConsoleDefaultColor;
        public event NewLogItemEventHandler NewLogItem;
        public delegate void NewLogItemEventHandler(LogItem logItem);
        public event ErrorOccurredEventHandler ErrorOccurred;
        public FixedSizeQueue<LogItem> LastLogEntries { get; private set; } = new FixedSizeQueue<LogItem>(1000);
        public delegate void ErrorOccurredEventHandler(Exception exception, LogItem logItem);
        private static uint _LoggerCounter = 0;
        private readonly string _LoggerId;
        public uint LoggerNumber { get; private set; }
        public string? Name { get; private set; }
        private bool AnyLogTargetEnabled
        {
            get
            {
                foreach (GRYLogTarget target in this.Configuration.LogTargets)
                {
                    if (target.Enabled)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        private GRYLog() : this(new GRYLogConfiguration(), null)
        {
        }
        private GRYLog(IGRYLogConfiguration configuration, string? name)
        {
            lock (_LockObject)
            {
                _LoggerCounter = _LoggerCounter + 1;
                this.Name = name;
                this._LoggerId = (this.Name == null ? this.GetType().Name + _LoggerCounter.ToString() : this.Name);
                this.LoggerNumber = _LoggerCounter;
                this._ConsoleDefaultColor = System.Console.ForegroundColor;
                this.Configuration = configuration;
                this.AddLogEntry = this.Log;
                this._Initialized = true;
            }
        }
        public static GRYLog Create(string? name = null, bool verbose = false, bool useGRYLogFormat = true, bool useNameAsSubnamespace = false)
        {
            GRYLog result = Create(new GRYLogConfiguration(true), name);
            foreach (GRYLogTarget target in result.Configuration.LogTargets)
            {
                if (verbose)
                {
                    target.LogLevels.Add(LogLevel.Debug);
                }
                if (useGRYLogFormat)
                {
                    target.Format = GRYLogLogFormat.GRYLogFormat;
                }
            }
            return result;
        }

        public static GRYLog Create(string logFile, string? name, string? basePath)
        {
            var result = Create(GRYLogConfiguration.GetCommonConfiguration(logFile, false), name);
            if (basePath == null)
            {
                result.BasePath = Path.GetDirectoryName(logFile);
            }
            else
            {
                result.BasePath = basePath;
            }
            return result;
        }

        public static GRYLog Create(IGRYLogConfiguration configuration, string? name, bool useNameAsSubnamespace = false)
        {
            GRYLog result = new GRYLog(configuration, name);
            if (useNameAsSubnamespace)
            {
                string subnamespace = GRYLibrary.Core.Misc.Utilities.GetValue(name);
                result.UseSubNamespace(subnamespace);
            }
            //FIXME result.BasePath must be set or logfile-targets must be disabled.
            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Configuration);
        }

        public override bool Equals(object obj)
        {
            if (obj is not GRYLog typedObject)
            {
                return false;
            }
            if (!this.Configuration.Equals(typedObject.Configuration))
            {
                return false;
            }
            return true;
        }
        /// <remarks>
        /// Will only be used if <see cref="GRYLogConfiguration.StoreProcessedLogItemsInternally"/> is set to true.ls -la
        /// </remarks>
        public IList<LogItem> ProcessedLogItems { get; set; } = [];
        public Action<LogItem> AddLogEntry { get; set; }

        public uint AmountOfErrors => throw new NotImplementedException();

        public uint AmountOfWarnings => throw new NotImplementedException();

        public uint GetAmountOfErrors()
        {
            return this._AmountOfErrors;
        }

        public uint GetAmountOfWarnings()
        {
            return this._AmountOfWarnings;
        }

        public void LogGeneralProgramInformation()
        {
            ProcessModule module = Process.GetProcessById(Environment.ProcessId).MainModule;
            this.Log($"Executing assembly-name: {AppDomain.CurrentDomain.FriendlyName} ({module.FileName})", LogLevel.Information);
            this.Log($"Executing file-version: {module.FileVersionInfo.FileVersion}", LogLevel.Information);
            this.Log($"Current working-directory: {Directory.GetCurrentDirectory()}", LogLevel.Information);
        }
        public void LogCurrentSystemStatistics()
        {
            //TODO refactor or assert the current system is windows
            this.Log($"OS description: {RuntimeInformation.OSDescription}", LogLevel.Information);
            string appDrive = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
            string cDrive = "C:\\";
            this.LogDriveStatistics(cDrive);
            if (!appDrive.Equals(cDrive))
            {
                this.LogDriveStatistics(appDrive);
            }
        }

        private void LogDriveStatistics(string drive)
        {
            this.Log($"Total free bytes on drive '{drive}': {Utilities.GetTotalFreeSpace(drive)}", LogLevel.Information);//todo print ram usage//todo print cpu usage//todo print if internetconnection exists
        }

        public void LogSummary()
        {
            this.Log($"Amount of occurred Errors and Criticals: {this.GetAmountOfErrors()}", LogLevel.Information);
            this.Log($"Amount of occurred Warnings: {this.GetAmountOfWarnings()}", LogLevel.Information);
        }

        private bool LineShouldBePrinted(string message)
        {
            if (message == null)
            {
                return false;
            }

            if (this.Configuration.PrintEmptyLines)
            {
                return true;
            }
            else
            {
                return !string.IsNullOrWhiteSpace(message);
            }
        }


        public void Log(Exception exception)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), () => "An error occurred.", exception, LogLevel.Information));
        }
        public void Log(string message)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), () => message, null, LogLevel.Information));
        }
        public void Log(string message, LogLevel logLevel)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), () => message, null, logLevel));
        }

        public void Log(string message, Exception exception)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), () => message, exception, LogLevel.Error));
        }
        public void Log(string message, Exception exception, LogLevel logLevel)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), () => message, exception, logLevel));
        }

        public void Log(Func<string> message, LogLevel logLevel)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), message, null, logLevel));
        }

        public void Log(Func<string> message, Exception exception)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), message, exception, LogLevel.Error));
        }
        public void Log(Func<string> getMessageFunction, Exception? exception, LogLevel logLevel)
        {
            this.Log(new LogItem(this.GetTimeForLogItem(), getMessageFunction, exception, logLevel));
        }
        public void Log(LogItem logitem)
        {
            lock (_LockObject)
            {
                if (this.Configuration.WriteLogEntriesAsynchronous)
                {
                    new Task(() => this.LogImplementation(logitem)).Start();
                }
                else
                {
                    this.LogImplementation(logitem);
                }
            }
        }

        private DateTimeOffset GetTimeForLogItem()
        {
            DateTimeOffset result = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
            return result;
        }
        public void LogProgramOutput(string message, string[] stdOutLines, string[] stdErrLines, LogLevel logevel)
        {
            lock (_LockObject)
            {
                this.Log(FormatProgramOutput(message, stdOutLines, stdErrLines), logevel);
            }
        }

        public static string FormatProgramOutput(string message, string[] stdOutLines, string[] stdErrLines)
        {
            return $"{message}; StdOut: {Environment.NewLine}{string.Join(Environment.NewLine, stdOutLines)}; StdErr: {Environment.NewLine}{string.Join(Environment.NewLine, stdErrLines)}";
        }


        private void LogImplementation(LogItem logItem)
        {
            try
            {
                lock (_LockObject)
                {
                    if (LogLevel.None == logItem.LogLevel)
                    {
                        return;
                    }
                    if (!this._Initialized)
                    {
                        return;
                    }
                    if (!this.Configuration.Enabled)
                    {
                        return;
                    }
                    if (!this.IsEnabled(logItem.LogLevel))
                    {
                        return;
                    }
                    if (!this.LineShouldBePrinted(logItem.PlainMessage))
                    {
                        return;
                    }
                    if (this.Configuration.StoreProcessedLogItemsInternally)
                    {
                        this.ProcessedLogItems.Add(logItem);
                    }
                    if (logItem.PlainMessage.Contains(Environment.NewLine) && this.Configuration.LogEveryLineOfLogEntryInNewLine)
                    {
                        foreach (string line in logItem.PlainMessage.Split([Environment.NewLine], StringSplitOptions.None))
                        {
                            this.Log(new LogItem(this.GetTimeForLogItem(), line, logItem.Exception, logItem.LogLevel));
                        }
                        return;
                    }
                    if (this.Configuration.PrintErrorsAsInformation && logItem.LogLevel.Equals(LogLevel.Error))
                    {
                        logItem.LogLevel = LogLevel.Information;
                    }
                    if (this.IsErrorLogLevel(logItem.LogLevel))
                    {
                        this._AmountOfErrors += 1;
                    }
                    if (this.IsWarningLogLevel(logItem.LogLevel))
                    {
                        this._AmountOfWarnings += 1;
                    }
                    this.LastLogEntries.Enqueue(logItem);
                    foreach (GRYLogTarget logTarget in this.Configuration.LogTargets)
                    {
                        if (logTarget.Enabled)
                        {
                            if (logItem.LogTargets.Contains(logTarget))
                            {

                                if (logTarget.LogLevels.Contains(logItem.LogLevel))
                                {
                                    logTarget.Execute(logItem, this);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorOccurred?.Invoke(exception, logItem);
            }
        }

        private bool IsWarningLogLevel(LogLevel logLevel)
        {
            return logLevel.Equals(LogLevel.Warning);
        }

        private bool IsErrorLogLevel(LogLevel logLevel)
        {
            return logLevel.Equals(LogLevel.Error) || logLevel.Equals(LogLevel.Critical);
        }

        [Obsolete($"Use {nameof(GeneralLoggerExtensions)}.{nameof(GeneralLoggerExtensions.LogLoopExecution)} instead.")]
        public void ExecuteAndLogForEach<T>(IEnumerable<T> items, Action<T> itemAction, string nameOfEntireLoopAction, string subNamespaceOfEntireLoopAction, Func<T, string> nameOfSingleItemFunc, Func<T, string> subNamespaceOfSingleItemFunc, bool preventThrowingExceptions = false, LogLevel logLevelForOverhead = LogLevel.Debug)
        {
            GeneralLoggerExtensions.Log(this, nameOfEntireLoopAction, logLevelForOverhead, !preventThrowingExceptions, true, false, true, true, () =>
            {
                List<T> itemsAsList = [.. items];
                uint amountOfItems = (uint)itemsAsList.Count;
                for (uint currentIndex = 0; currentIndex < itemsAsList.Count; currentIndex++)
                {
                    try
                    {
                        T currentItem = itemsAsList[(int)currentIndex];
                        string nameOfSingleItem = nameOfSingleItemFunc(currentItem);
                        GeneralLoggerExtensions.Log(this, nameOfSingleItemFunc(currentItem), logLevelForOverhead, !preventThrowingExceptions, true, false, true, false, () => itemAction(currentItem));
                    }
                    finally
                    {
                        this.LogProgress(currentIndex + 1, amountOfItems);
                    }
                }
            });
        }

        public void LogProgress(uint enumerator, uint denominator)
        {
            if (enumerator < 0 || denominator < 0 || denominator < enumerator)
            {
                throw new ArgumentException($"Can not log progress for {nameof(enumerator)}={enumerator} and {nameof(denominator)}={denominator}. Both values must be nonnegative and {nameof(denominator)} must be greater than {nameof(enumerator)}.");
            }
            else
            {
                string percentValue = Math.Round(enumerator / (double)denominator * 100, 2).ToString();
                int denominatorLength = (int)Math.Floor(Math.Log10(denominator) + 1);
                string message = $"Processed {enumerator.ToString().PadLeft(denominatorLength, '0')}/{denominator} items ({percentValue}%)";
                this.Log(message);
            }
        }
        [Obsolete($"Use {nameof(GeneralLoggerExtensions)}.{nameof(GeneralLoggerExtensions.Log)} instead")]
        public void ExecuteAndLog(Action action, string nameOfAction, bool preventThrowingExceptions = false, LogLevel logLevelForOverhead = LogLevel.Debug, string subNamespaceForLog = Utilities.EmptyString)
        {
            GeneralLoggerExtensions.Log(this, nameOfAction, logLevelForOverhead, !preventThrowingExceptions, true, false, true, true, action);
        }

        public TResult ExecuteAndLog<TResult>(Func<TResult> action, string nameOfAction, bool preventThrowingExceptions = false, LogLevel logLevelForOverhead = LogLevel.Debug, TResult defaultValue = default, string subNamespaceForLog = Utilities.EmptyString)
        {
            this.Log($"Action '{nameOfAction}' will be started now.", logLevelForOverhead);
            Stopwatch stopWatch = new();
            try
            {
                using (this.UseSubNamespace(subNamespaceForLog))
                {
                    stopWatch.Start();
                    TResult result = action();
                    stopWatch.Stop();
                    return result;
                }
            }
            catch (Exception exception)
            {
                this.Log($"An exception occurred while executing action '{nameOfAction}'.", exception, LogLevel.Error);
                if (preventThrowingExceptions)
                {
                    return defaultValue;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                this.Log($"Action '{nameOfAction}' finished. Duration: {Utilities.DurationToUserFriendlyString(stopWatch.Elapsed)}", logLevelForOverhead);
            }
        }

        public void Dispose()
        {
        }

        public IDisposable UseSubNamespace(string subnamespace)
        {
            return new GRYLogSubNamespaceProvider(this, subnamespace);
        }

        internal void InvokeObserver(LogItem message)
        {
            try
            {
                NewLogItem?.Invoke(message);
            }
            catch
            {
                Utilities.NoOperation();
            }
        }

        private string FormatEvent(EventId eventId)
        {
            return $"EventId: {eventId.Id}, EventName: '{eventId.Name}'";
        }

        #region ILogger-Implementation

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.AnyLogTargetEnabled;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new GRYLogSubNamespaceProvider(this, state.ToString());
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.Log(() => $"{this.FormatEvent(eventId)} | {formatter(state, exception)}", logLevel);
        }


        #endregion
        public string GetLoggerId()
        {
            return this._LoggerId;
        }

        public void ApplyConfiguration(IGRYLogConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void RunTask(Action action, string taskName, bool catchErrors, LogLevel logLevelForOverhead)
        {
            this.Log($"Run task {taskName}", logLevelForOverhead);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (catchErrors)
                {
                    this.Log($"An error occurred while running task {taskName}.", ex, LogLevel.Error);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                this.Log($"Finished task {taskName}", logLevelForOverhead);
            }
        }
    }
}