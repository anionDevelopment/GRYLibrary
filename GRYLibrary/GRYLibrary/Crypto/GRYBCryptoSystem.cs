using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.Crypto
{
    /// <summary>
    /// The <see cref="GRYBCryptoSystem"/> is a cryptosystem to encrypt data with an optional masterkey to be able to decrypt the data without the password.
    /// The "B" in the name stands for "backdoor" since the masterpassword is obviously a backdoor.
    /// The purpose of this cryptosystem is to be able to encrypt data conformable to law when you have the requirement that a third party (and only this third party) must be able to decrypt the data.
    /// </summary>
    /// <remarks>
    /// The size of the entire encrypted data is:
    /// length(encrypted payload data)+(little overhead)
    /// (little overhead)=sum(length(publickey) for each publickey in <see cref="PasswordEncryptionKeys"/>)+(some very small technical overhead)
    /// So if there are n keys available in <see cref="PasswordEncryptionKeys"/> then the size of the entire encrypted data is not n*(encrypted payload data)+(little overhead) but 1*(encrypted payload data)+(little overhead).
    /// </remarks>
    /// <example>
    /// Example-usage:
    /// It is a statutory requirement that the Secret service s can also decrypt the data.
    /// The secret service creates a master key mk (mk is technically a private key for an RSA-keypair).
    /// Company c want to encrypt userdata with a user-specific password p. p can be defined by the c or by the user or can be a user-client-side generated password. (p is technically a private key for an RSA-keypair.) c does neither have to know p nor mk.
    /// Then the public keys of mk and p will be added to <see cref="PasswordEncryptionKeys"/>.
    /// Now c can encrypt and decrypt data with gcs.<see cref="CommonEncryptionAlgorithm.Encrypt(byte[], byte[])"/> and gcs.<see cref="Decrypt(byte[], byte[])"/>.
    /// The encrypted data can be decrypted either with the password used when calling <see cref="CommonEncryptionAlgorithm.Encrypt(byte[], byte[])"/> or (by s) using mp as password.
    /// (In General: The data can be decrypted with all keys which are contained in <see cref="PasswordEncryptionKeys"/> when <see cref="CommonEncryptionAlgorithm.Encrypt(byte[], byte[])"/> is called.)
    /// </example>
    public class GRYBCryptoSystem : EncryptUsingPropertiesAlgorithm
    {
        public const string GRYBCryptoSystemDatasetMagicHeader = "GRYBC";
        public static readonly byte[] GRYBCryptoSystemDatasetMagicHeaderBytes = System.Text.Encoding.ASCII.GetBytes(GRYBCryptoSystemDatasetMagicHeader);
        public ISet<(byte[], AsymmetricEncryptionAlgorithm)> PasswordEncryptionKeys { get; set; } = new HashSet<(byte[], AsymmetricEncryptionAlgorithm)>();
        public SymmetricEncryptionAlgorithm InternalEncryptionAlgorithmForKeys { get; set; } = new AES256();
        public HashAlgorithm HashAlgorithm { get; set; } = new SHA256();

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] unencryptedData)
        {
            return EncryptAndSerialize(unencryptedData, [.. this.PasswordEncryptionKeys], this.InternalEncryptionAlgorithmForKeys, this.HashAlgorithm);
        }

        /// <inheritdoc/>
        /// <param name="password">
        /// As password you can either use any private key of a public key which was contained in <see cref="PasswordEncryptionKeys"/> when calling <see cref="CommonEncryptionAlgorithm.Encrypt(byte[], byte[])"/>.
        /// </param>
        public override byte[] Decrypt(byte[] encryptedData, byte[] password)
        {
            return DeserializeAndDecrypt(encryptedData, password);
        }

        public static byte[] EncryptAndSerialize(byte[] unencryptedData, (byte[], AsymmetricEncryptionAlgorithm)[] unencryptedPublicPasswordEncryptionKeys, SymmetricEncryptionAlgorithm internalEncryptionAlgorithmForKeys, HashAlgorithm hashAlgorithm)
        {
            byte[] internalKey = internalEncryptionAlgorithmForKeys.GenerateRandomKey();
            (byte[], AsymmetricEncryptionAlgorithm)[] encryptedKeysForDecryption = [.. unencryptedPublicPasswordEncryptionKeys.Select(unencryptedPublicPasswordEncryptionKey => (unencryptedPublicPasswordEncryptionKey.Item2.Encrypt(internalKey, unencryptedPublicPasswordEncryptionKey.Item1), unencryptedPublicPasswordEncryptionKey.Item2))];
            byte[] content = internalEncryptionAlgorithmForKeys.Encrypt(unencryptedData, internalKey);
            byte[] header = Utilities.Concat(hashAlgorithm.GetIdentifier(), internalEncryptionAlgorithmForKeys.GetIdentifier(), Utilities.UnsignedInteger32BitToByteArray((uint)encryptedKeysForDecryption.Length), Utilities.UnsignedInteger32BitToByteArray((uint)content.Length));
            foreach ((byte[], AsymmetricEncryptionAlgorithm) encryptedKeyForDecryption in encryptedKeysForDecryption)
            {
                header = Utilities.Concat(header, encryptedKeyForDecryption.Item2.GetIdentifier(), Utilities.UnsignedInteger32BitToByteArray((uint)encryptedKeyForDecryption.Item1.Length), encryptedKeyForDecryption.Item1);
            }
            byte[] data = Utilities.Concat(header, content);
            byte[] hashed = hashAlgorithm.Hash(data);
            return Utilities.Concat(GRYBCryptoSystemDatasetMagicHeaderBytes, Utilities.UnsignedInteger32BitToByteArray((uint)hashed.Length), hashed, data);
        }

        public static byte[] DeserializeAndDecrypt(byte[] encryptedData, byte[] password)
        {
            if (!IsValidDataset(encryptedData))
            {
                throw new ArgumentException($"The provided data does not represent a valid {nameof(GRYBCryptoSystem)}-dataset");
            }
            throw new NotImplementedException();
        }
        public static bool IsValidDataset(byte[] encryptedData)
        {
            try
            {
                if (!Utilities.StartsWith(encryptedData, GRYBCryptoSystemDatasetMagicHeaderBytes))
                {
                    return false;
                }
                //TODO add other checks
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static GRYBCryptoSystem CreateGRYBCryptoSystemForCommonUsage((byte[], AsymmetricEncryptionAlgorithm) publicKeyOfMasterKey, (byte[], AsymmetricEncryptionAlgorithm) publicKeyOfUserKey)
        {
            GRYBCryptoSystem result = new();
            result.PasswordEncryptionKeys.Add(publicKeyOfMasterKey);
            result.PasswordEncryptionKeys.Add(publicKeyOfUserKey);
            return result;
        }

        public override byte[] GetIdentifier()
        {
            return Utilities.PadLeft(GRYBCryptoSystemDatasetMagicHeaderBytes, 10);
        }
    }
}