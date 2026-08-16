using System;
using System.Collections;
using System.Collections.Generic;
namespace GRYLibrary.Core.AOA.SerializeHelper
{
    internal class GRYSet<T> : ISet<T>
    {
        private bool _AllowDuplicatedElements = true;
        private ISet<T> _ItemsAsSet = null;
        private readonly IList<T> _ItemsAsList = [];
        internal void DisallowDuplicatedElements()
        {
            if (this._AllowDuplicatedElements)
            {
                this._AllowDuplicatedElements = false;
                this._ItemsAsSet = new HashSet<T>(this._ItemsAsList);
                this._ItemsAsList.Clear();
            }
        }
        public int Count
        {
            get
            {
                if (this._AllowDuplicatedElements)
                {
                    return this._ItemsAsList.Count;
                }
                else
                {
                    return this._ItemsAsSet.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this._AllowDuplicatedElements)
                {
                    return this._ItemsAsList.IsReadOnly;
                }
                else
                {
                    return this._ItemsAsSet.IsReadOnly;
                }
            }
        }

        public bool Add(T item)
        {
            if (this._AllowDuplicatedElements)
            {
                this._ItemsAsList.Add(item);
                return true;
            }
            else
            {
                return this._ItemsAsSet.Add(item);
            }
        }

        public void Clear()
        {
            if (this._AllowDuplicatedElements)
            {
                this._ItemsAsList.Clear();
            }
            else
            {
                this._ItemsAsSet.Clear();
            }
        }

        public bool Contains(T item)
        {
            if (this._AllowDuplicatedElements)
            {
                return this._ItemsAsList.Contains(item);
            }
            else
            {
                return this._ItemsAsSet.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (this._AllowDuplicatedElements)
            {
                this._ItemsAsList.CopyTo(array, arrayIndex);
            }
            else
            {
                this._ItemsAsSet.CopyTo(array, arrayIndex);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                this._ItemsAsSet.ExceptWith(other);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this._AllowDuplicatedElements)
            {
                return this._ItemsAsList.GetEnumerator();
            }
            else
            {
                return this._ItemsAsSet.GetEnumerator();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                this._ItemsAsSet.IntersectWith(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.IsProperSubsetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.IsProperSupersetOf(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.IsSupersetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.Overlaps(other);
            }
        }

        public bool Remove(T item)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotImplementedException();
            }
            else
            {
                return this._ItemsAsSet.Remove(item);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this._ItemsAsSet.SetEquals(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                this._ItemsAsSet.SymmetricExceptWith(other);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (this._AllowDuplicatedElements)
            {
                throw new NotSupportedException();
            }
            else
            {
                this._ItemsAsSet.UnionWith(other);
            }
        }

        void ICollection<T>.Add(T item)
        {
            if (this._AllowDuplicatedElements)
            {
                this._ItemsAsList.Add(item);
            }
            else
            {
                this._ItemsAsSet.Add(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this._AllowDuplicatedElements)
            {
                return this._ItemsAsList.GetEnumerator();
            }
            else
            {
                return this._ItemsAsSet.GetEnumerator();
            }
        }
    }
}