using GRYLibrary.Core.Misc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GRYLibrary.Core.AOA.EqualsHelper
{
    public class ReferenceEqualsComparer : IEqualityComparer<object>
    {
        public new bool Equals(object item1, object item2)
        {
            bool result = Utilities.ImprovedReferenceEquals(item1, item2);
            return result;
        }

        public int GetHashCode(object obj)
        {
            int result = RuntimeHelpers.GetHashCode(obj);
            return result;
        }
    }
}