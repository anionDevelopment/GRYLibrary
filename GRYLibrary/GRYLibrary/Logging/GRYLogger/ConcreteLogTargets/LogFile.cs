using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.FilePath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets
{
    public sealed class LogFile : GRYLogTarget
    {
        public LogFile() { }
        public AbstractFilePath File { get; set; }
        public string Encoding { get; set; } = "utf-8";
        public uint MaxLogFileSizeInBytes { get; set; } = 0;
        private readonly IList<string> _Pool = [];
        public int PreFlushPoolSize { get; set; } = 1;
        private string _BasePath;
        protected override void ExecuteImplementation(LogItem logItem, GRYLog logObject)
        {
            this._BasePath = logObject.BasePath;
            logItem.Format(logObject.Configuration, out string formattedMessage, out int _, out int _, out ConsoleColor _, this.Format);
            this._Pool.Add(formattedMessage);
            if (this.PreFlushPoolSize <= this._Pool.Count)
            {
                this.Flush();
            }
            string logfile = this.File.GetPath(this._BasePath);
            if (this.MaxLogFileSizeInBytes != 0)
            {
                if (new FileInfo(logfile).Length > this.MaxLogFileSizeInBytes)
                {
                    //TODO do log-rotate
                    string rotatedLogFile = this.GetRotatedLogFileName(logfile);
                    System.IO.File.Move(logfile, rotatedLogFile);
                    GRYLibrary.Core.Misc.Utilities.EnsureFileExists(logfile);
                }
            }
        }

        private string GetRotatedLogFileName(string logfile)//TODO refactor to prevent while-loop
        {
            uint counter = 1;
            string result = this.GetRotatedLogFile(logfile, counter);
            while (System.IO.File.Exists(result))
            {
                counter += 1;
                result = this.GetRotatedLogFile(logfile, counter);
            }
            return result;
        }

        private string GetRotatedLogFile(string logfile, uint counter)
        {
            string folder = Path.GetDirectoryName(logfile);
            string filename = Path.GetFileNameWithoutExtension(logfile);
            string result = Path.Combine(folder, $"{filename}.archive.{counter.ToString().PadLeft(6, '0')}.log");
            return result;
        }

        public void Flush()
        {
            if (string.IsNullOrWhiteSpace(this._BasePath))
            {
                throw new NullReferenceException($"{nameof(this._BasePath)} is not defined.");
            }
            string logfile = this.File.GetPath(this._BasePath);
            Utilities.EnsureFileExists(logfile, true);
            string result = string.Empty;
            for (int i = 0; i < this._Pool.Count; i++)
            {
                if (!(i == 0 && Utilities.FileIsEmpty(logfile)))
                {
                    result += Environment.NewLine;
                }
                result += this._Pool[i];
            }
            Encoding encoding = Utilities.GetEncodingByIdentifier(this.Encoding);
            System.IO.File.AppendAllText(logfile, result, encoding);
            this._Pool.Clear();
        }

        public override HashSet<Type> FurtherGetExtraTypesWhichAreRequiredForSerialization()
        {
            return [];
        }

        public override void Dispose()
        {
            this.Flush();
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