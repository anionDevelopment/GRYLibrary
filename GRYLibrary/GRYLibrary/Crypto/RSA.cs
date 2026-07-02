using GRYLibrary.Core.Misc;
using System.Security.Cryptography;

namespace GRYLibrary.Core.Crypto
{
    public class RSA : AsymmetricEncryptionAlgorithm
    {
        public int KeyLengthForNewGeneratedKeys { get; set; } = 4096;
        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] encryptedData, byte[] privateKey)
        {
            using RSACryptoServiceProvider rsaCryptoServiceProvider = new();
            rsaCryptoServiceProvider.ImportParameters(PasswordToRSAParameters(privateKey));
            return rsaCryptoServiceProvider.Decrypt(encryptedData, true);
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] unencryptedData, byte[] publicKey)
        {
            using RSACryptoServiceProvider rsaCryptoServiceProvider = new();
            rsaCryptoServiceProvider.ImportParameters(PasswordToRSAParameters(publicKey));
            return rsaCryptoServiceProvider.Encrypt(unencryptedData, true);
        }

        public static RSAParameters PasswordToRSAParameters(byte[] bytes)
        {
            byte[][] parameters = Utilities.GetBytesArraysFromConcatBytesArraysWithLengthInformation(bytes);
            return new RSAParameters()
            {
                D = parameters[0],
                DP = parameters[1],
                Exponent = parameters[2],
                InverseQ = parameters[3],
                Modulus = parameters[4],
                P = parameters[5],
                Q = parameters[6],
            };
        }
        public static byte[] RSAParametersToPassword(RSAParameters parameters)
        {
            return Utilities.ConcatBytesArraysWithLengthInformation(parameters.D, parameters.DP, parameters.Exponent, parameters.InverseQ, parameters.Modulus, parameters.P, parameters.Q);
        }

        /// <inheritdoc/>
        public override (byte[] privateKey, byte[] publicKey) GenerateRandomKeyPair()
        {
            using RSACryptoServiceProvider rsaCryptoServiceProvider = new(this.KeyLengthForNewGeneratedKeys);
            return (RSAParametersToPassword(rsaCryptoServiceProvider.ExportParameters(true)), RSAParametersToPassword(rsaCryptoServiceProvider.ExportParameters(false)));
        }

        /// <inheritdoc/>
        public override byte[] GetIdentifier()
        {
            return Utilities.PadLeft(System.Text.Encoding.ASCII.GetBytes(nameof(RSA)), 10);
        }

        public override byte[] SignData(byte[] data, byte[] key, HashAlgorithm hashAlgorithm)
        {
            throw new System.NotImplementedException();
        }
    }
}