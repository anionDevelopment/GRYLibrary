using GRYLibrary.Core.Misc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets
{
    public sealed class Console : GRYLogTarget
    {
        public bool WriteWarningsToStdErr { get; set; } = true;
        public bool WriteEntireOutputToStdOut { get; set; } = false;

        public bool UseColors { get; set; } = !Utilities.RunningInContainer;
        private static readonly object _Lock = new object();
        public Console() { }
        protected override void ExecuteImplementation(LogItem logItem, GRYLog logObject)
        {
            TextWriter output;
            if ((logItem.IsErrorEntry() || (this.WriteWarningsToStdErr && logItem.LogLevel == LogLevel.Warning)) && !this.WriteEntireOutputToStdOut)
            {
                output = System.Console.Error;
            }
            else
            {
                output = System.Console.Out;
            }
            logItem.Format(logObject.Configuration, out string formattedMessage, out int cb, out int ce, out ConsoleColor _, this.Format);
            string part1 = formattedMessage.AsSpan(0, cb).ToString();
            string part2 = formattedMessage[cb..ce];
            string part3 = formattedMessage[ce..] + Environment.NewLine;
            lock (_Lock)
            {
                if (this.UseColors)
                {
                    output.Write(part1);
                    this.WriteWithColorToConsole(part2, output, logItem.LogLevel, logObject);
                    output.Write(part3);
                }
                else
                {
                    output.Write(part1 + part2 + part3);
                }
            }
            output.Flush();
        }
        public override HashSet<Type> FurtherGetExtraTypesWhichAreRequiredForSerialization()
        {
            return [typeof(ConsoleColor)];
        }

        private void WriteWithColorToConsole(string message, TextWriter output, LogLevel logLevel, GRYLog logObject)
        {
            if (message.Length > 0)
            {
                try
                {
                    System.Console.ForegroundColor = logObject.Configuration.GetLoggedMessageTypesConfigurationByLogLevel(logLevel).ConsoleColor;
                    output.Write(message);
                }
                finally
                {
                    System.Console.ForegroundColor = logObject._ConsoleDefaultColor;
                }
            }
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

        public void EnableColors()
        {
            this.UseColors = true;
        }

        public void DisableColors()
        {
            this.UseColors = false;
        }
    }
}