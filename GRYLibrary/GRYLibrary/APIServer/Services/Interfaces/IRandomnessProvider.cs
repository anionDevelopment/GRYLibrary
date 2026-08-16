namespace GRYLibrary.Core.APIServer.Services.Interfaces
{
    /// <summary>
    /// Represents a randomness provider.
    /// </summary>
    public interface IRandomnessProvider
    {
        /// <returns>Returns a non-negative number which is lower than <paramref name="maximalValue"/>.</returns>
        public int Next(int maximalValue);
        // <summary>
        // Fills the array with random bytes.
        // </summary>
        void NextBytes(byte[] buffer);
    }
}
