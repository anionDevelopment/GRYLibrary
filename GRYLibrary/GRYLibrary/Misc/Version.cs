//https://stackoverflow.com/a/2085890/3905529
using System;
using System.Globalization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Serializable version of the System.Version class.
    /// </summary>
    /// <remarks>
    /// <see cref="Major"/> and <see cref="Minor"/> are always set. <see cref="Build"/> and <see cref="Revision"/> are optional:
    /// they are <see langword="null"/> if and only if they are not set. (In contrast to <see cref="System.Version"/> the value -1
    /// is not used as marker for "not set", because all parts of a version are non-negative by definition.)
    /// </remarks>
    [Serializable]
    public class Version : ICloneable, IComparable
    {
        /// <summary>
        /// Gets or sets the major.
        /// </summary>
        public uint Major { get; set; }
        /// <summary>
        /// Gets or sets the minor.
        /// </summary>
        public uint Minor { get; set; }
        /// <summary>
        /// Gets or sets the build or <see langword="null"/> if the build is not set.
        /// </summary>
        public uint? Build { get; set; }
        /// <summary>
        /// Gets or sets the revision or <see langword="null"/> if the revision is not set.
        /// </summary>
        public uint? Revision { get; set; }

        /// <returns>Returns true if and only if <see cref="Build"/> is set.</returns>
        public bool HasBuild => this.Build.HasValue;

        /// <returns>Returns true if and only if <see cref="Revision"/> is set.</returns>
        public bool HasRevision => this.Revision.HasValue;

        /// <summary>
        /// Creates a new <see cref="Version"/> instance which represents the version "0.0".
        /// </summary>
        public Version()
        {
            this.Major = 0;
            this.Minor = 0;
            this.Build = null;
            this.Revision = null;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="version">Version. Must consist of 2, 3 or 4 non-negative numbers which are separated by dots.</param>
        public Version(string version)
        {
            if (version == null)
            {
                throw Utilities.CreateNullReferenceExceptionDueToParameter(nameof(version));
            }
            string[] parts = version.Split('.');
            if (parts.Length is < 2 or > 4)
            {
                throw new ArgumentException($"'{version}' is not a valid version because it does not consist of 2, 3 or 4 parts.", nameof(version));
            }
            this.Major = ParsePart(parts[0], version, nameof(this.Major));
            this.Minor = ParsePart(parts[1], version, nameof(this.Minor));
            this.Build = 2 < parts.Length ? ParsePart(parts[2], version, nameof(this.Build)) : null;
            this.Revision = 3 < parts.Length ? ParsePart(parts[3], version, nameof(this.Revision)) : null;
        }

        private static uint ParsePart(string part, string version, string partName)
        {
            if (uint.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out uint result))
            {
                return result;
            }
            throw new ArgumentException($"'{version}' is not a valid version because '{part}' is not a valid value for the {partName}.", nameof(version));
        }

        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        public Version(int major, int minor)
        {
            this.Major = ToVersionPart(major, nameof(major));
            this.Minor = ToVersionPart(minor, nameof(minor));
            this.Build = null;
            this.Revision = null;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="build">Build.</param>
        public Version(int major, int minor, int build)
        {
            this.Major = ToVersionPart(major, nameof(major));
            this.Minor = ToVersionPart(minor, nameof(minor));
            this.Build = ToVersionPart(build, nameof(build));
            this.Revision = null;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="build">Build.</param>
        /// <param name="revision">Revision.</param>
        public Version(int major, int minor, int build, int revision)
        {
            this.Major = ToVersionPart(major, nameof(major));
            this.Minor = ToVersionPart(minor, nameof(minor));
            this.Build = ToVersionPart(build, nameof(build));
            this.Revision = ToVersionPart(revision, nameof(revision));
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <remarks>
        /// <see cref="System.Version"/> uses the value -1 to mark the build and the revision as not set.
        /// Such a value is transferred to <see langword="null"/> and not treated as error.
        /// </remarks>
        public Version(System.Version version)
        {
            if (version == null)
            {
                throw Utilities.CreateNullReferenceExceptionDueToParameter(nameof(version));
            }
            this.Major = ToVersionPart(version.Major, nameof(version.Major));
            this.Minor = ToVersionPart(version.Minor, nameof(version.Minor));
            this.Build = ToOptionalVersionPart(version.Build, nameof(version.Build));
            this.Revision = ToOptionalVersionPart(version.Revision, nameof(version.Revision));
        }

        private static uint ToVersionPart(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "A part of a version can not be negative.");
            }
            return (uint)value;
        }

        private static uint? ToOptionalVersionPart(int value, string parameterName)
        {
            if (value == -1)
            {
                return null;//-1 is the marker of System.Version for "not set"
            }
            return ToVersionPart(value, parameterName);
        }

        #region ICloneable Members
        /// <summary>
        /// Clones this instance.
        /// </summary>
        public object Clone()
        {
            return new Version
            {
                Major = this.Major,
                Minor = this.Minor,
                Build = this.Build,
                Revision = this.Revision
            };
        }
        #endregion
        #region IComparable Members
        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }
            if (version is not Version typedVersion)
            {
                throw new ArgumentException($"Object must be of type {nameof(Version)}.", nameof(version));
            }
            int result = this.Major.CompareTo(typedVersion.Major);
            if (result != 0)
            {
                return result;
            }
            result = this.Minor.CompareTo(typedVersion.Minor);
            if (result != 0)
            {
                return result;
            }
            // Nullable.Compare treats "not set" as lower than every set value, so "1.2" is lower than "1.2.0".
            result = Nullable.Compare(this.Build, typedVersion.Build);
            if (result != 0)
            {
                return result;
            }
            return Nullable.Compare(this.Revision, typedVersion.Revision);
        }
        #endregion
        /// <summary>
        /// Equalss the specified obj.
        /// </summary>
        /// <param name="obj">Obj.</param>
        public override bool Equals(object obj)
        {
            if (obj is not Version typedObject)
            {
                return false;
            }
            return this.Major == typedObject.Major
                && this.Minor == typedObject.Minor
                && this.Build == typedObject.Build
                && this.Revision == typedObject.Revision;
        }
        /// <summary>
        /// Gets the hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Major, this.Minor, this.Build, this.Revision);
        }
        /// <summary>
        /// Operator ==s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator ==(Version v1, Version v2)
        {
            if (v1 is null)
            {
                return v2 is null;
            }
            return v1.Equals(v2);
        }
        /// <summary>
        /// Operator !=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }
        /// <summary>
        /// Operator &lt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator <(Version v1, Version v2)
        {
            if (v1 is null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.CompareTo(v2) < 0;
        }
        /// <summary>
        /// Operator &lt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator <=(Version v1, Version v2)
        {
            if (v1 is null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.CompareTo(v2) <= 0;
        }
        /// <summary>
        /// Operator &gt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator >(Version v1, Version v2)
        {
            return v2 < v1;
        }
        /// <summary>
        /// Operator &gt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static bool operator >=(Version v1, Version v2)
        {
            return v2 <= v1;
        }
        /// <remarks>
        /// Only the fields which are set are transferred, because <see cref="System.Version"/> does not accept a build or a revision
        /// which is not set together with a set successor-field.
        /// </remarks>
        public System.Version ToSystemVersion()
        {
            if (!this.HasBuild)
            {
                return new System.Version(this.ToInt32(this.Major, nameof(this.Major)), this.ToInt32(this.Minor, nameof(this.Minor)));
            }
            if (!this.HasRevision)
            {
                return new System.Version(this.ToInt32(this.Major, nameof(this.Major)), this.ToInt32(this.Minor, nameof(this.Minor)), this.ToInt32(this.Build.Value, nameof(this.Build)));
            }
            return new System.Version(this.ToInt32(this.Major, nameof(this.Major)), this.ToInt32(this.Minor, nameof(this.Minor)), this.ToInt32(this.Build.Value, nameof(this.Build)), this.ToInt32(this.Revision.Value, nameof(this.Revision)));
        }

        /// <remarks><see cref="System.Version"/> uses <see cref="int"/> for its parts, so values which are too large can not be transferred.</remarks>
        private int ToInt32(uint value, string partName)
        {
            if (int.MaxValue < value)
            {
                throw new OverflowException($"The {partName} of the version '{this}' is too large to be represented by a {nameof(System)}.{nameof(System.Version)}.");
            }
            return (int)value;
        }

        /// <summary>
        /// Toes the string.
        /// </summary>
        public override string ToString()
        {
            if (!this.HasBuild)
            {
                return this.ToString(2);
            }
            if (!this.HasRevision)
            {
                return this.ToString(3);
            }
            return this.ToString(4);
        }
        /// <summary>
        /// Toes the string.
        /// </summary>
        /// <param name="fieldCount">Field count.</param>
        public string ToString(int fieldCount)
        {
            switch (fieldCount)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return this.Major.ToString(CultureInfo.InvariantCulture);
                case 2:
                    return $"{this.Major}.{this.Minor}";
                case 3:
                    if (!this.HasBuild)
                    {
                        throw new ArgumentException($"The {nameof(this.Build)} is not set, so a maximum of 2 fields can be printed.", nameof(fieldCount));
                    }
                    return $"{this.Major}.{this.Minor}.{this.Build.Value}";
                case 4:
                    if (!this.HasBuild)
                    {
                        throw new ArgumentException($"The {nameof(this.Build)} is not set, so a maximum of 2 fields can be printed.", nameof(fieldCount));
                    }
                    if (!this.HasRevision)
                    {
                        throw new ArgumentException($"The {nameof(this.Revision)} is not set, so a maximum of 3 fields can be printed.", nameof(fieldCount));
                    }
                    return $"{this.Major}.{this.Minor}.{this.Build.Value}.{this.Revision.Value}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldCount), fieldCount, "A version can only be printed with 0, 1, 2, 3 or 4 fields.");
            }
        }
    }
}
