using System.Text;

namespace GRYLibrary.Core.Crypto
{
    public abstract class CryptographyAlgorithm
    {
        /// <summary>
        /// Represents an identifier for this algorithm.
        /// </summary>
        /// <returns>
        /// Returns an identifier with en exact length of 10.
        /// </returns>
        public abstract byte[] GetIdentifier();
        public Encoding Encoding { get; } = new UTF8Encoding(false);
    }
}