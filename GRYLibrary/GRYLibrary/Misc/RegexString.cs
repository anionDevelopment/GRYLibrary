using System;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.Misc
{
    public class RegexString
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
                if (this._Regex.IsMatch(value))
                {
                    this._Value = value;
                }
                else
                {
                    throw new ArgumentException($"Value \"{this._Value}\" is not assignable because it does not match the regex \"{this._Regex}\".");
                }
            }
        }
        private Regex _Regex;
        public Regex GetRegex()
        {
            return this._Regex;
        }

        public void SetRegex(Regex regex)
        {
            this._Regex = regex;
        }
    }
}
