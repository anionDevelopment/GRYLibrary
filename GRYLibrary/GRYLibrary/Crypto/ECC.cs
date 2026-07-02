using GRYLibrary.Core.Misc;
using System;

namespace GRYLibrary.Core.Crypto
{
    public class ECC : AsymmetricEncryptionAlgorithm
    {
        public Curve Curve { get; set; } = Curve.Curve25519;
        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] encryptedData, byte[] password)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] unencryptedData, byte[] password)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override (byte[] privateKey, byte[] publicKey) GenerateRandomKeyPair()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override byte[] GetIdentifier()
        {
            return Utilities.PadLeft(System.Text.Encoding.ASCII.GetBytes(nameof(ECC)), 10);
        }

        public override byte[] SignData(byte[] data, byte[] key, HashAlgorithm hashAlgorithm)
        {
            throw new NotImplementedException();
        }
    }
}