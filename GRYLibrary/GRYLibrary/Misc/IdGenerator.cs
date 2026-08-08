using System;
using System.Collections.Generic;
using System.Numerics;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Simple generic generator for ids.
    /// </summary>
    /// <typeparam name="T">Represents the type of an id</typeparam>
    public interface IIdGenerator<T>
    {
        public T GenerateNewId();
        public ISet<T> GeneratedIds();
        void Reset();
        void Reset(T lastValue);
    }
    public class IdGenerator<T> : IIdGenerator<T>
    {
        private readonly ISet<T> _GeneratedIds = new HashSet<T>();
        private readonly Func<T, T> _GenerateNewId;
        T LastId = default;
        private readonly Func<T> _reset;
        public IdGenerator(Func<T, T> generateNewId, Func<T> reset)
        {
            this._GenerateNewId = generateNewId;
            this._reset = reset;
        }
        public T GenerateNewId()
        {
            this.LastId = this._GenerateNewId(this.LastId);
            this._GeneratedIds.Add(this.LastId);
            return this.LastId;
        }
        public ISet<T> GeneratedIds()
        {
            return new HashSet<T>(this._GeneratedIds);
        }

        public void Reset()
        {
            this.LastId = this._reset();
        }

        public void Reset(T lastValue)
        {
            this.LastId = lastValue;
        }
    }
    public static class IdGenerator
    {
        /// <summary>
        /// Represents an id-generator which generates increasing ids beginning with 1.
        /// </summary>
        /// <remarks>The value 0 is the state before the first id was generated and is therefore never returned as id.</remarks>
        public static IdGenerator<int> GetDefaultIntIdGenerator()
        {
            return new IdGenerator<int>((int lastGeneratedId) => lastGeneratedId + 1, () => 0);
        }

        /// <summary>
        /// Represents an id-generator which generates increasing ids beginning with 1.
        /// </summary>
        /// <remarks>The value 0 is the state before the first id was generated and is therefore never returned as id.</remarks>
        public static IdGenerator<ulong> GetDefaultLongIdGenerator()
        {
            return new IdGenerator<ulong>((ulong lastGeneratedId) => lastGeneratedId + 1, () => 0);
        }

        /// <summary>
        /// Represents an id-generator which generates increasing ids beginning with 1.
        /// </summary>
        /// <remarks>The value 0 is the state before the first id was generated and is therefore never returned as id.</remarks>
        public static IdGenerator<BigInteger> GetDefaultBigIntegerIdGenerator()
        {
            return new IdGenerator<BigInteger>((BigInteger lastGeneratedId) => lastGeneratedId + 1, () => 0);
        }

        /// <summary>
        /// Represents an id-generator which generates random guids.
        /// </summary>
        public static IdGenerator<Guid> GetDefaultGuidIdGenerator()
        {
            return new IdGenerator<Guid>((Guid lastGeneratedId) => Guid.NewGuid(), Guid.NewGuid);
        }
    }
}