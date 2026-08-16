using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public class GenericComparer<T> : IComparer<T>, IEqualityComparer<T>
    {
        private readonly Func<T, T, int> _Comparer;
        private readonly Func<T, int> _GetHashCode;
        public GenericComparer(Func<T, T, int> comparer, Func<T, int> getHashCode)
        {
            this._Comparer = comparer;
            this._GetHashCode = getHashCode;
        }
        public static IComparer<T> CreateComparer(Func<T, T, int> comparer)
        {
            return CreateComparer(comparer, (_) => 0);
        }

        public static IEqualityComparer<T> CreateEqualityComparer(Func<T, T, int> comparer)
        {
            return CreateEqualityComparer(comparer, (_) => 0);
        }

        public static IComparer<T> CreateComparer(Func<T, T, int> comparer, Func<T, int> getHashCode)
        {
            return new GenericComparer<T>(comparer, getHashCode);
        }

        public static IEqualityComparer<T> CreateEqualityComparer(Func<T, T, int> comparer, Func<T, int> getHashCode)
        {
            return new GenericComparer<T>(comparer, getHashCode);
        }

        public int Compare(T x, T y)
        {
            return this._Comparer(x, y);
        }

        public bool Equals(T x, T y)
        {
            return this._Comparer(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return this._GetHashCode(obj);
        }
    }
}