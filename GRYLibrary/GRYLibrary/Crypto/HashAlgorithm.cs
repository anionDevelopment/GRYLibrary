using GRYLibrary.Core.Misc;

namespace GRYLibrary.Core.Crypto
{
    public abstract class HashAlgorithm : CryptographyAlgorithm
    {
        public abstract byte[] Hash(byte[] data);
        public virtual string Hash(string data)
        {
            return Utilities.ByteArrayToHexString(this.Hash(this.Encoding.GetBytes(data)));
        }
    }
}