using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc.Migration
{
    public class MigrationInstance : IEquatable<MigrationInstance>
    {
        public MigrationInstance(uint index, string migrationName, string migrationContent)
        {
            this.Index = index;
            this.MigrationName = migrationName;
            this.MigrationContent = migrationContent;
        }

        public uint Index { get; set; }
        public string MigrationName { get; set; }
        public string MigrationContent { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MigrationInstance);
        }

        public bool Equals(MigrationInstance other)
        {
            return other is not null &&
                   this.Index == other.Index &&
                   this.MigrationName == other.MigrationName &&
                   this.MigrationContent == other.MigrationContent;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Index, this.MigrationName, this.MigrationContent);
        }

        public static bool operator ==(MigrationInstance left, MigrationInstance right)
        {
            return EqualityComparer<MigrationInstance>.Default.Equals(left, right);
        }

        public static bool operator !=(MigrationInstance left, MigrationInstance right)
        {
            return !(left == right);
        }
    }
}
