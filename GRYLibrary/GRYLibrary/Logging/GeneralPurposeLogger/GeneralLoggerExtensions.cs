using Microsoft.Extensions.Logging;
using System;
using GUtilies = GRYLibrary.Core.Misc.Utilities;
using System.Diagnostics;
using GRYLibrary.Core.Logging.GRYLogger;
using System.Collections.Generic;
using GRYLibrary.Core.Misc;

namespace GRYLibrary.Core.Logging.GeneralPurposeLogger
{
    public static class GeneralLoggerExtensions
    {

        public static void Log(this IGeneralLogger logger, string actionName, LogLevel logLevelForOverhead, bool throwExceptionIfOccurrs, bool logStartOfAction, bool logExceptionOfAtion, bool logEndOfAtion, bool printDuration, Action action)
        {
            if (logStartOfAction)
            {
                logger.Log($"Start action \"{actionName}\".", logLevelForOverhead);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                stopwatch.Start();
                action();
                stopwatch.Stop();
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                if (logExceptionOfAtion)
                {
                    logger.Log($"Error in action \"{actionName}\".", exception);
                }
                if (throwExceptionIfOccurrs)
                {
                    throw;
                }
            }
            finally
            {
                if (logEndOfAtion)
                {
                    string duration;
                    if (printDuration)
                    {
                        duration = $" Duration: {GUtilies.DurationToUserFriendlyString(stopwatch.Elapsed)}";
                    }
                    else
                    {
                        duration = GUtilies.EmptyString;
                    }
                    logger.Log($"Finished action \"{actionName}\".{duration}", logLevelForOverhead);
                }
            }
        }
        public static void LogLoopExecution<T>(this IGeneralLogger logger, string title, IEnumerable<T> items, bool continueOnError, Func<T, string> getName, LogLevel loglevelForOverhead, Action<T> action)
        {
            logger.Log(GUtilies.LongLine, loglevelForOverhead);
            logger.Log($"Run '{title}' for {items.Count()} items.", loglevelForOverhead);

            logger.Log(GUtilies.Line, loglevelForOverhead);
            foreach (T? item in items)
            {
                string name = getName(item);
                logger.Log($"Start action for item '{name}'.", loglevelForOverhead);
                try
                {
                    action(item);
                }
                catch (Exception exception)
                {
                    logger.Log($"Error occurred while doing action for item '{name}'.", exception);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
                finally
                {
                    logger.Log($"Finished action for item '{name}'.", loglevelForOverhead);
                }
                logger.Log(GUtilies.Line, loglevelForOverhead);
            }
            logger.Log($"Finished '{title}'.", loglevelForOverhead);
            logger.Log(GUtilies.LongLine, loglevelForOverhead);
        }


        public static IGRYLog SetupLogger(GRYLogConfiguration configuration, string basePath, string subnamespace, string name)
        {
            GRYLog result = GRYLog.Create(configuration, name, false);
            result.UseSubNamespace(subnamespace);
            result.BasePath = basePath;
            return result;
        }
    }
}
