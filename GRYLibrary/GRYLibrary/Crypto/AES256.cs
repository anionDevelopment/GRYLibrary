using GRYLibrary.Core.Misc;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GRYLibrary.Core.Crypto
{
    public class AES256 : SymmetricEncryptionAlgorithm
    {
        #region AES
        private const int _IVLength = 16;
#pragma warning disable SYSLIB0022 // Typ oder Element ist veraltet
        private const int _AESBlockLength = 16;

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] data, byte[] key)
        {
            string plainText = Utilities.ByteArrayToHexString(data);
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                if (aesAlg.IV.Length != _IVLength)
                {
                    throw new ArgumentException($"Expected IV-length {_IVLength} but was {aesAlg.IV.Length}.");
                }
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
                encrypted = [.. aesAlg.IV, .. encrypted];
            }
            return encrypted;
        }
        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] data, byte[] key)
        {
            string cipherText = Utilities.ByteArrayToHexString(data);
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }

            byte[] iv = [.. data.Take(_IVLength)];
            data = [.. data.Skip(_IVLength)];
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("IV");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using MemoryStream msDecrypt = new MemoryStream(data);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }
            return Utilities.HexStringToByteArray(plaintext);
        }

        private byte[] GetIV()
        {
            using RijndaelManaged algorithmImplementation = new();
            algorithmImplementation.GenerateIV();
            if (algorithmImplementation.IV.Length != _AESBlockLength)
            {
                throw new InvalidDataException($"Expected length of IV to be {_AESBlockLength} but was {algorithmImplementation.IV.Length}");
            }
            return algorithmImplementation.IV;
        }

        /// <inheritdoc/>
        public override byte[] GenerateRandomKey()
        {
            using RijndaelManaged algorithmImplementation = new();
            algorithmImplementation.GenerateKey();
            return algorithmImplementation.Key;
        }
        public override byte[] GetIdentifier()
        {
            return Utilities.PadLeft(System.Text.Encoding.ASCII.GetBytes(nameof(AES256)), 10);
        }
#pragma warning restore SYSLIB0022 // Typ oder Element ist veraltet
        #endregion
    }
}