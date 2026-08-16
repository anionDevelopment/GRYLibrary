using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;

namespace GRYLibrary.Core.APIServer.Services.OtherServices
{
    /// <summary>
    /// Represents a randomness provider basically for testcases and other non-cryptographic-usecases which is deterministic when giving the same random every time.
    /// </summary>
    public class RandomnessProvider : IRandomnessProvider
    {
        private readonly Random _Random;
        public RandomnessProvider() : this(new Random(42))
        {
        }
        public RandomnessProvider(Random random)
        {
            this._Random = random;
        }
        /// <inheritdoc cref="IRandomnessProvider.Next"/>
        public int Next(int maximalValue)
        {
            return this._Random.Next(maximalValue);
        }

        /// <inheritdoc cref="IRandomnessProvider.NextBytes"/>
        public void NextBytes(byte[] buffer)
        {
            this._Random.NextBytes(buffer);
        }
    }
}
