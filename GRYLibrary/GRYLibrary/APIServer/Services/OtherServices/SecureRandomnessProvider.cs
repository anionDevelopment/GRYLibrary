using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Security.Cryptography;

namespace GRYLibrary.Core.APIServer.Services.OtherServices
{
    /// <summary>
    /// Represents a randomness provider which is cryptographically secure.
    /// </summary>
    /// <remarks>
    /// The cryptographic security is based on the cryptographic security of <see cref="RandomNumberGenerator"/>.
    /// </remarks>
    public class SecureRandomnessProvider : IRandomnessProvider
    {
        public SecureRandomnessProvider()
        {
        }
        /// <inheritdoc cref="IRandomnessProvider.Next"/>
        public int Next(int maximalValue)
        {
            return RandomNumberGenerator.GetInt32(maximalValue);
        }

        /// <inheritdoc cref="IRandomnessProvider.NextBytes"/>
        public void NextBytes(byte[] buffer)
        {
            Array.Copy(RandomNumberGenerator.GetBytes(buffer.Length), buffer, buffer.Length);
        }
    }
}
