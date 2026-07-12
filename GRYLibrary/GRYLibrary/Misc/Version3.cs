using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public class Version3 : IEquatable<Version3>
    {
        public ulong Major { get; set; }
        public ulong Minor { get; set; }
        public ulong Patch { get; set; }

        public Version3()
        {
        }
        public Version3(ulong major, ulong minor, ulong patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Version3);
        }

        public bool Equals(Version3 other)
        {
            return other is not null &&
                   this.Major == other.Major &&
                   this.Minor == other.Minor &&
                   this.Patch == other.Patch;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Major, this.Minor, this.Patch);
        }

        public static bool operator ==(Version3 left, Version3 right)
        {
            return EqualityComparer<Version3>.Default.Equals(left, right);
        }

        public static bool operator !=(Version3 left, Version3 right)
        {
            return !(left == right);
        }

        public static bool operator <(Version3 left, Version3 right)
        {
            if (left.Major < right.Major)
            {
                return true;
            }
            else if (left.Major == right.Major)
            {
                if (left.Minor < right.Minor)
                {
                    return true;
                }
                else if (left.Minor == right.Minor)
                {
                    return left.Patch < right.Patch;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool operator >(Version3 left, Version3 right)
        {
            return right < left;
        }
        public static bool operator <=(Version3 left, Version3 right)
        {
            return !(left > right);
        }
        public static bool operator >=(Version3 left, Version3 right)
        {
            return !(left < right);
        }

        public override string ToString()
        {
            return $"{this.Major}.{this.Minor}.{this.Patch}";
        }

        public static Version3 Parse(string applicationVersion)
        {
            string[] splitted = applicationVersion.Split('.');
            return new Version3(ushort.Parse(splitted[0]), ushort.Parse(splitted[1]), ushort.Parse(splitted[2]));
        }
    }
}
