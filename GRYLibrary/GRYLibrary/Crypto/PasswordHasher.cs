using System;
using System.Security.Cryptography;
using System.Text;

namespace GRYLibrary.Core.Crypto
{
    /// <summary>
    /// Hashes and verifies passwords using PBKDF2 (HMAC-SHA256) with a per-password random salt and key-stretching.
    /// This is the correct primitive for storing a password, as opposed to a fast hash like <see cref="SHA256"/>.
    /// </summary>
    /// <remarks>
    /// The produced hash is a self-describing string of the form
    /// <c>PBKDF2-SHA256$&lt;iterations&gt;$&lt;salt-base64&gt;$&lt;hash-base64&gt;</c>.
    /// Because the algorithm and its parameters are stored next to the hash, the iteration-count can be raised later
    /// without invalidating already-stored hashes: an old hash still verifies against its own stored parameters, and a
    /// consumer can re-hash it with the current parameters on the next successful login.
    /// </remarks>
    public class PasswordHasher
    {
        /// <summary>The iteration-count used for hashes which this instance produces (following the OWASP-recommendation for PBKDF2-HMAC-SHA256).</summary>
        public const int DefaultIterations = 600000;
        private const int SaltLengthInBytes = 16;
        private const int HashLengthInBytes = 32;
        private const string AlgorithmIdentifier = "PBKDF2-SHA256";
        private const char FieldSeparator = '$';

        private readonly int _Iterations;

        public PasswordHasher() : this(DefaultIterations)
        {
        }

        public PasswordHasher(int iterations)
        {
            if (iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "The iteration-count must be positive.");
            }
            this._Iterations = iterations;
        }

        /// <summary>
        /// Hashes <paramref name="password"/> with a freshly generated random salt and returns a self-describing hash-string
        /// which contains the algorithm, the iteration-count, the salt and the derived key.
        /// </summary>
        public string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            byte[] salt = RandomNumberGenerator.GetBytes(SaltLengthInBytes);
            byte[] hash = this.DeriveKey(password, salt, this._Iterations);
            return AlgorithmIdentifier + FieldSeparator + this._Iterations + FieldSeparator + Convert.ToBase64String(salt) + FieldSeparator + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies <paramref name="password"/> against a hash-string which was produced by <see cref="Hash"/>.
        /// The comparison is done in constant time to not leak information through its duration.
        /// Returns <see langword="false"/> for a hash-string which is not in the expected format instead of throwing.
        /// </summary>
        public bool Verify(string password, string hashString)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrEmpty(hashString))
            {
                return false;
            }
            string[] parts = hashString.Split(FieldSeparator);
            if (parts.Length != 4 || parts[0] != AlgorithmIdentifier)
            {
                return false;
            }
            if (!int.TryParse(parts[1], out int iterations) || iterations < 1)
            {
                return false;
            }
            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }
            byte[] actualHash = this.DeriveKey(password, salt, iterations, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private byte[] DeriveKey(string password, byte[] salt, int iterations)
        {
            return this.DeriveKey(password, salt, iterations, HashLengthInBytes);
        }

        private byte[] DeriveKey(string password, byte[] salt, int iterations, int outputLength)
        {
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, outputLength);
        }
    }
}
