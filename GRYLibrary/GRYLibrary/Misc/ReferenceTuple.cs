using System;

namespace GRYLibrary.Core.Misc
{
    public class ReferenceTuple : IEquatable<ReferenceTuple>
    {
        public object Item1 { get; set; }
        public object Item2 { get; set; }
        public ReferenceTuple(object item1, object item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ReferenceTuple);
        }

        /// <summary>
        /// Returns a hash-code which follows the same rule as <see cref="Equals(ReferenceTuple)"/>: the identity of an
        /// item which is a reference and the value of an item which is not.
        /// </summary>
        /// <remarks>
        /// A hash-code which is the same for every tuple would be allowed but useless: a set of such tuples puts all of
        /// them into one bucket and compares a new one against every one which is already in it. That set is used to
        /// remember which pairs of objects were already compared, so it grows with every comparison - and the
        /// bookkeeping would then cost more than the comparisons it saves.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                return (GetHashCodeOfAnItem(this.Item1) * 397) ^ GetHashCodeOfAnItem(this.Item2);
            }
        }

        private static int GetHashCodeOfAnItem(object item)
        {
            if (item == null)
            {
                return 0;
            }
            if (item.GetType().IsValueType)
            {
                return item.GetHashCode();
            }
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item);
        }

        public bool Equals(ReferenceTuple other)
        {
            return other != null
                && Utilities.ImprovedReferenceEquals(this.Item1, other.Item1)
                && Utilities.ImprovedReferenceEquals(this.Item2, other.Item2);
        }
    }
}