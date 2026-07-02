using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    public class LoggedMessageTypeConfiguration
    {
        public ConsoleColor ConsoleColor { get; set; }
        public string CustomText { get; set; }
        public LoggedMessageTypeConfiguration() { }
        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
        }

        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }

        public override string ToString()
        {
            return Generic.GenericToString(this);
        }
        #endregion
    }
}