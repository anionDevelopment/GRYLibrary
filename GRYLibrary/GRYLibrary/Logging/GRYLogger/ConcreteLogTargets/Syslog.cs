using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets
{
    public sealed class Syslog : GRYLogTarget
    {

        protected override void ExecuteImplementation(LogItem logItem, GRYLog logObject)
        {
            string messageId = string.Empty;

            using ExternalProgramExecutor externalProgramExecutor = new("Logger", $"--tag {Utilities.GetNameOfCurrentExecutable()} {messageId} -- [{logItem.LogLevel}] [{logObject.Configuration.Name}] {logItem.PlainMessage}");
            externalProgramExecutor.Run();
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