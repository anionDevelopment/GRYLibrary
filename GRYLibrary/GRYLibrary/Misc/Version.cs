//https://stackoverflow.com/a/2085890/3905529
using System;
using System.Globalization;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Serializable version of the System.Version class.
    /// </summary>
    [Serializable]
    public class Version : ICloneable, IComparable
    {
        private int major;
        private int minor;
        private int build;
        private int revision;
        /// <summary>
        /// Gets the major.
        /// </summary>
        /// <value></value>
        public int Major
        {
            get => this.major;
            set => this.major = value;
        }
        /// <summary>
        /// Gets the minor.
        /// </summary>
        /// <value></value>
        public int Minor
        {
            get => this.minor;
            set => this.minor = value;
        }
        /// <summary>
        /// Gets the build.
        /// </summary>
        /// <value></value>
        public int Build
        {
            get => this.build;
            set => this.build = value;
        }
        /// <summary>
        /// Gets the revision.
        /// </summary>
        /// <value></value>
        public int Revision
        {
            get => this.revision;
            set => this.revision = value;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        public Version()
        {
            this.build = -1;
            this.revision = -1;
            this.major = 0;
            this.minor = 0;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="version">Version.</param>
        public Version(string version)
        {
            this.build = -1;
            this.revision = -1;
            if (version == null)
            {
                throw Utilities.CreateNullReferenceExceptionDueToParameter(nameof(version));
            }
            char[] chArray1 = ['.'];
            string[] textArray1 = version.Split(chArray1);
            int num1 = textArray1.Length;
            if (num1 is < 2 or > 4)
            {
                throw new ArgumentException("Arg_VersionString");
            }
            this.major = int.Parse(textArray1[0], CultureInfo.InvariantCulture);
            if (this.major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "ArgumentOutOfRange_Version");
            }
            this.minor = int.Parse(textArray1[1], CultureInfo.InvariantCulture);
            if (this.minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "ArgumentOutOfRange_Version");
            }
            num1 -= 2;
            if (num1 > 0)
            {
                this.build = int.Parse(textArray1[2], CultureInfo.InvariantCulture);
                if (this.build < 0)
                {
                    throw new ArgumentOutOfRangeException("build", "ArgumentOutOfRange_Version");
                }
                num1--;
                if (num1 > 0)
                {
                    this.revision = int.Parse(textArray1[3], CultureInfo.InvariantCulture);
                    if (this.revision < 0)
                    {
                        throw new ArgumentOutOfRangeException("revision", "ArgumentOutOfRange_Version");
                    }
                }
            }
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        public Version(int major, int minor)
        {
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.major = major;
        }
        /// <summary>
        /// Creates a new <see cref="Version"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="build">Build.</param>
        public Version(int major, int minor, int build)
        {
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), "ArgumentOutOfRange_Version");
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(build), "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.build = build;
        }
        public Version(System.Version version) : this(version.Major, version.Minor, version.Build, version.Revision)
        {
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
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), "ArgumentOutOfRange_Version");
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(build), "ArgumentOutOfRange_Version");
            }
            if (revision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(revision), "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.build = build;
            this.revision = revision;
        }
        #region ICloneable Members
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Version version1 = new Version
            {
                major = this.major,
                minor = this.minor,
                build = this.build,
                revision = this.revision
            };
            return version1;
        }
        #endregion
        #region IComparable Members
        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }
            if (version is not Version)
            {
                throw new ArgumentException("Arg_MustBeVersion");
            }
            Version version1 = (Version)version;
            if (this.major != version1.Major)
            {
                if (this.major > version1.Major)
                {
                    return 1;
                }
                return -1;
            }
            if (this.minor != version1.Minor)
            {
                if (this.minor > version1.Minor)
                {
                    return 1;
                }
                return -1;
            }
            if (this.build != version1.Build)
            {
                if (this.build > version1.Build)
                {
                    return 1;
                }
                return -1;
            }
            if (this.revision == version1.Revision)
            {
                return 0;
            }
            if (this.revision > version1.Revision)
            {
                return 1;
            }
            return -1;
        }
        #endregion
        /// <summary>
        /// Equalss the specified obj.
        /// </summary>
        /// <param name="obj">Obj.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is null or not Version)
            {
                return false;
            }
            Version version1 = (Version)obj;
            if ((this.major == version1.Major) && (this.minor == version1.Minor) && (this.build == version1.Build) && (this.revision == version1.Revision))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int num1 = 0;
            num1 |= (this.major & 15) << 0x1c;
            num1 |= (this.minor & 0xff) << 20;
            num1 |= (this.build & 0xff) << 12;
            return num1 | (this.revision & 0xfff);
        }
        /// <summary>
        /// Operator ==s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator ==(Version v1, Version v2)
        {
            return v1.Equals(v2);
        }
        /// <summary>
        /// Operator &gt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >(Version v1, Version v2)
        {
            return v2 < v1;
        }
        /// <summary>
        /// Operator &gt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >=(Version v1, Version v2)
        {
            return v2 <= v1;
        }
        /// <summary>
        /// Operator !=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator !=(Version v1, Version v2)
        {
            return v1 != v2;
        }
        /// <summary>
        /// Operator &lt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator <(Version v1, Version v2)
        {
            if (v1 == null)
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
        /// <returns></returns>
        public static bool operator <=(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.CompareTo(v2) <= 0;
        }
        public System.Version ToSystemVersion()
        {
            return new System.Version(this.major, this.minor, this.build, this.revision);
        }

        /// <summary>
        /// Toes the string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.build == -1)
            {
                return this.ToString(2);
            }
            if (this.revision == -1)
            {
                return this.ToString(3);
            }
            return this.ToString(4);
        }
        /// <summary>
        /// Toes the string.
        /// </summary>
        /// <param name="fieldCount">Field count.</param>
        /// <returns></returns>
        public string ToString(int fieldCount)
        {
            object[] objArray1;
            switch (fieldCount)
            {
                case 0:
                    {
                        return string.Empty;
                    }
                case 1:
                    {
                        return this.major.ToString();
                    }
                case 2:
                    {
                        return this.major.ToString() + "." + this.minor.ToString();
                    }
            }
            if (this.build == -1)
            {
                throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "2"), nameof(fieldCount));
            }
            if (fieldCount == 3)
            {
                objArray1 = [this.major, ".", this.minor, ".", this.build];
                return string.Concat(objArray1);
            }
            if (this.revision == -1)
            {
                throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "3"), nameof(fieldCount));
            }
            if (fieldCount == 4)
            {
                objArray1 = [this.major, ".", this.minor, ".", this.build, ".", this.revision];
                return string.Concat(objArray1);
            }
            throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "4"), nameof(fieldCount));
        }
    }
}