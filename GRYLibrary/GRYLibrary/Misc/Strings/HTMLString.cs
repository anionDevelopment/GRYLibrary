using GRYLibrary.Core.Exceptions;
using System;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.Strings
{
    public class HTMLString
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
                throw new InvalidFormatException($"The given value is invalid as {nameof(HTMLString)}.");
            }
        }

        public static bool ValueIsValid(string value)
        {
            if (GUtilities.HasDangerousCharacters(value))
            {
                return false;
            }
            if (!GUtilities.IsValidHTML(value))
            {
                return false;
            }
            return true;
        }

        public static HTMLString From(string value)
        {
            return new HTMLString() { Value = value };
        }

        public override string ToString()
        {
            throw new NotSupportedException($"Not supported. Please use the {nameof(this.Value)}-property to access the value of this {this.GetType().FullName}.");
        }
    }
}
