using System;
using System.Xml.Serialization;

namespace GRYLibrary.Core.Misc.FilePath
{
    [XmlInclude(typeof(AbsoluteFilePath))]
    [XmlInclude(typeof(RelativeFilePath))]
    public abstract class AbstractFilePath
    {
        public abstract string FilePath { get; set; }

        public static AbstractFilePath FromString(string? logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                return new NoPathGiven();
            }
            if (Utilities.IsAbsoluteLocalFilePath(logFile))
            {
                return new AbsoluteFilePath() { FilePath = logFile };
            }
            if (Utilities.IsRelativeLocalFilePath(logFile))
            {
                return new RelativeFilePath() { FilePath = logFile };
            }
            throw new ArgumentException($"Cannot calculate path-type of path '{logFile}'.");
        }

        public abstract string GetPath();
        public abstract string GetPath(string basePath);
    }
}
