using GRYLibrary.Core.Exceptions;
using System;
using System.Collections.Generic;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.Strings
{
    public class OneLineString
    {
        private string _Value;
        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                EnsureValueIsValid(value);
                this._Value = value;
            }
        }

        public static void EnsureValueIsValid(string value)
        {
            if (!ValueIsValid(value))
            {
                throw new InvalidFormatException($"The given value is invalid as {nameof(OneLineString)}.");
            }
        }

        public static bool ValueIsValid(string value)
        {
            if (value.Contains('\n'))
            {
                return false;
            }
            if (value.Contains('\r'))
            {
                return false;
            }
            if (GUtilities.HasDangerousCharacters(value))
            {
                return false;
            }
            return true;
        }

        public static OneLineString From(string value)
        {
            return new OneLineString() { Value = value };
        }

        public string ToHTMLString()
        {
            return GUtilities.HTMLUnescape(this._Value);
        }

        public override bool Equals(object obj)
        {
            return obj is OneLineString @string &&
                   this._Value == @string._Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this._Value);
        }

        public static bool operator ==(OneLineString left, OneLineString right)
        {
            return EqualityComparer<OneLineString>.Default.Equals(left, right);
        }

        public static bool operator !=(OneLineString left, OneLineString right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            throw new NotSupportedException($"Not supported. Please use the {nameof(this.Value)}-property to access the value of this {this.GetType().FullName}.");
        }
    }
}
