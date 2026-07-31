using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents a byte-array which can easily used in some common formats (e. g. hex-string).
    /// </summary>
    /// <remarks>
    /// Objects of this type should be treated as unmodifiable.
    /// </remarks>
    public class ByteArray
    {
        public static Encoding DefaultEncoding { get; } = new UTF8Encoding(false);
        private readonly byte[] _Data;
        public byte[] Data => this._Data;
        public string DataAsHexString { get; }
        public ByteArray(byte[] value)
        {
            this._Data = value ?? throw new Exception("No data for byte-array available.");
            this.DataAsHexString = Utilities.ByteArrayToHexString(this._Data);
        }
        public ByteArray CreateByHexString(string value)
        {
            return new ByteArray(Utilities.HexStringToByteArray(value));
        }

        public ByteArray CreateByInteger(BigInteger value)
        {
            return this.CreateByHexString(Utilities.BigIntegerToHexString(value));
        }

        public ByteArray CreateByString(string value)
        {
            return this.CreateByString(value, DefaultEncoding);
        }

        public ByteArray CreateByString(string value, Encoding encoding)
        {
            return new ByteArray(encoding.GetBytes(value));
        }

        public string DecodeToString()
        {
            return this.DecodeToString(DefaultEncoding);
        }

        public string DecodeToString(Encoding encoding)
        {
            return encoding.GetString(this._Data);
        }

        public static ByteArray operator +(ByteArray first, ByteArray second)
        {
            return new ByteArray(Utilities.Concat(first._Data, second._Data));
        }
        public static ByteArray operator +(ByteArray first, byte second)
        {
            return first + new byte[] { second };
        }
        public static ByteArray operator +(ByteArray first, byte[] second)
        {
            return first + new ByteArray(second);
        }
        public static ByteArray operator -(ByteArray first, ByteArray second)
        {
            if (first._Data.Length != second._Data.Length)
            {
                throw new ArgumentException("Subtraction of Byte-Arrays is only possible for Byte-Arrays of same length.");
            }
            byte[] result = new byte[first._Data.Length];
            for (int i = 0; i < result.Length; i++)
            {
                byte x = first._Data[i];
                byte y = second._Data[i];
                int z = x - y;
                if (z < 0)
                {
                    z += 256;
                }
                result[i] = (byte)z;
            }
            return new ByteArray(result);
        }
        public static bool operator ==(ByteArray first, ByteArray second)
        {
            return first.Equals(second);
        }
        public static bool operator !=(ByteArray first, ByteArray second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            ByteArray other = obj as ByteArray;
            return other != null && Enumerable.SequenceEqual(this._Data, other._Data);
        }
        public override int GetHashCode()
        {
            //The hash-code must be calculated based on the content because Equals also compares the content and not the reference.
            HashCode hashCode = new();
            hashCode.AddBytes(this._Data);
            return hashCode.ToHashCode();
        }

        public bool StartsWith(ByteArray value)
        {
            if (value._Data.LongLength > this._Data.LongLength)
            {
                return false;
            }
            for (long i = 0; i < value._Data.LongLength; i++)
            {
                if (this._Data[i] != value._Data[i])
                {
                    return false;
                }
            }
            return true;
        }
        public bool Contains(ByteArray value)
        {
            return this._Data.ToList().ContainsSublist([.. value._Data], (@byte) => @byte.ToString());
        }
    }
}