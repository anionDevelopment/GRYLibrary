using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc.Migration
{
    public class MigrationExecutionInformation : IEquatable<MigrationExecutionInformation>
    {
        public string MigrationName { get; set; }
        public DateTime ExecutionTimestamp { get; set; }

        public MigrationExecutionInformation(string migrationName, DateTime executionTimestamp)
        {
            this.MigrationName = migrationName;
            this.ExecutionTimestamp = executionTimestamp;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MigrationExecutionInformation);
        }

        public bool Equals(MigrationExecutionInformation other)
        {
            return other is not null &&
                   this.MigrationName == other.MigrationName &&
                   this.ExecutionTimestamp == other.ExecutionTimestamp;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.MigrationName, this.ExecutionTimestamp);
        }

        public static bool operator ==(MigrationExecutionInformation left, MigrationExecutionInformation right)
        {
            return EqualityComparer<MigrationExecutionInformation>.Default.Equals(left, right);
        }

        public static bool operator !=(MigrationExecutionInformation left, MigrationExecutionInformation right)
        {
            return !(left == right);
        }
    }
}
