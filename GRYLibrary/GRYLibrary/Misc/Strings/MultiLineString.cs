using GRYLibrary.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.Strings
{
    public class MultiLineString
    {
        private string _Value;
        private bool _ReplaceCRAutomatically;

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (this._ReplaceCRAutomatically)
                {
                    this._Value = ReplaceCR(value);
                }
                else
                {
                    EnsureValueIsValid(value);
                    this._Value = value;
                }
            }
        }
        public static void EnsureValueIsValid(string value)
        {
            if (!ValueIsValid(value))
            {
                throw new InvalidFormatException($"The given value is invalid as {nameof(MultiLineString)}.");
            }
        }

        public static bool ValueIsValid(string value)
        {
            if (GUtilities.HasDangerousCharacters(value))
            {
                return false;
            }
            return true;
        }

        private static string ReplaceCR(string value)
        {
            value = value.Replace("\r", string.Empty);
            //TODO this function can probably be improved to handle lineendings for example like Notepad++.
            return value;
        }

        public static MultiLineString From(string value)
        {
            return From(value, true);
        }

        public static MultiLineString From(string value, bool replaceCRAutomatically)
        {
            return new MultiLineString() { _ReplaceCRAutomatically = replaceCRAutomatically, Value = value, };
        }

        public override bool Equals(object obj)
        {
            return obj is MultiLineString @string &&
                   this._Value == @string._Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this._Value);
        }

        public string ToHTMLString()
        {
            return GUtilities.HTMLEscape(this._Value);
        }

        public static bool operator ==(MultiLineString left, MultiLineString right)
        {
            return EqualityComparer<MultiLineString>.Default.Equals(left, right);
        }

        public static bool operator !=(MultiLineString left, MultiLineString right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            throw new NotSupportedException($"Not supported. Please use the {nameof(this.Value)}-property to access the value of this {this.GetType().FullName}.");
        }

        public static MultiLineString FromHTML(string @value)
        {
            return From(WebUtility.HtmlDecode(@value));
        }
    }
}
