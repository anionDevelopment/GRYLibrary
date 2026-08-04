using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.Misc
{
    public class UnorderedList<T> : IList<T>, IEquatable<UnorderedList<T>>
    {
        private readonly IList<T> _Items = [];
        public int Count => this._Items.Count;

        public bool IsReadOnly => this._Items.IsReadOnly;

        public T this[int index] { get => this._Items[index]; set => this._Items[index] = value; }

        public override int GetHashCode()
        {
            return HashCode.Combine(this._Items.Count.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as UnorderedList<T>);
        }

        /// <returns>Returns true if and only if there is a bijection between this and <paramref name="other"/>.</returns>
        public bool Equals(UnorderedList<T> other)
        {
            return this.GetItemsWithCount(this).SetEquals(this.GetItemsWithCount(other));
        }

        private ISet<WriteableTuple<T, ulong>> GetItemsWithCount(UnorderedList<T> items)
        {
            Dictionary<T, ulong> result = [];
            foreach (T item in items)
            {
                if (result.ContainsKey(item))
                {
                    result[item] = result[item] + 1;
                }
                else
                {
                    result.Add(item, 1);
                }
            }
            return new HashSet<WriteableTuple<T, ulong>>(result.Select(kvp => new WriteableTuple<T, ulong>(kvp.Key, kvp.Value)));
        }

        public int IndexOf(T item)
        {
            return this._Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this._Items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._Items.RemoveAt(index);
        }

        public void Add(T item)
        {
            this._Items.Add(item);
        }

        public void Clear()
        {
            this._Items.Clear();
        }

        public bool Contains(T item)
        {
            return this._Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this._Items.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._Items.GetEnumerator();
        }
    }
}
