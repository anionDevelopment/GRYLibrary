using System.IO;
using System.Text;

namespace GRYLibrary.Core.Misc
{
    public class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding _Encoding;

        public override Encoding Encoding
        {
            get
            {
                if (this._Encoding == default)
                {
                    return base.Encoding;
                }
                else
                {
                    return this._Encoding;
                }
            }
        }

        public StringWriterWithEncoding(Encoding encoding)
        {
            this._Encoding = encoding;
        }
    }
}