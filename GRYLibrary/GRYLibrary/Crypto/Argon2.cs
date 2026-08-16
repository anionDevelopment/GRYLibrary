using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using System;

namespace GRYLibrary.Core.Crypto
{
    public class Argon2 : HashAlgorithm
    {
        public override byte[] Hash(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetIdentifier()
        {
            return GUtilities.PadLeft(Encoding.ASCII.GetBytes(nameof(Argon2)), 10);
        }
    }
}