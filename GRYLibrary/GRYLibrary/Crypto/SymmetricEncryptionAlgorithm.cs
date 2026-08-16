namespace GRYLibrary.Core.Crypto
{
    public abstract class SymmetricEncryptionAlgorithm : CommonEncryptionAlgorithm
    {
        /// <summary>
        /// Creates a new random key for encryption with this symmetric encryption algorithm.
        /// </summary>
        public abstract byte[] GenerateRandomKey();
    }
}