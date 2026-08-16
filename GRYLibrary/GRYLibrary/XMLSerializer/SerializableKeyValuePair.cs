using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Core.XMLSerializer
{
    /// <summary>
    /// Represents a key-value-pair which is serializable.
    /// </summary>
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public SerializableKeyValuePair()
        {
        }
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }

        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
        }

        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }

        public override string ToString()
        {
            return Generic.GenericToString(this);
        }
        #endregion
    }
}