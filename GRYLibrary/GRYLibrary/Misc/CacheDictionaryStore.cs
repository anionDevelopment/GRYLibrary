using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc
{
    public class CacheDictionaryStore<TKey, TValue, THelper>
    {
        internal readonly IDictionary<TKey, TValue> _Cache = new Dictionary<TKey, TValue>();
        internal readonly Func<TKey, THelper, TValue> _GetFunction = null;

        public CacheDictionaryStore(Func<TKey, THelper, TValue> getFunction)
        {
            this._GetFunction = getFunction;
        }

        public TValue GetValue(TKey key, THelper helper)
        {
            if (!this._Cache.TryGetValue(key, out TValue value))
            {
                value = this._GetFunction(key, helper);
                this._Cache.Add(key, value);
            }
            return value;
        }
        public void ResetCache()
        {
            this._Cache.Clear();
        }

        public void ResetCache(TKey item)
        {
            this._Cache.Remove(item);
        }

        public bool ContainsKey(TKey key)
        {
            return this._Cache.ContainsKey(key);
        }
    }
    public class CacheDictionaryStore<TKey, TValue>
    {
        private readonly CacheDictionaryStore<TKey, TValue, object> _CacheDictionaryStore;
        public CacheDictionaryStore(Func<TKey, TValue> getFunction)
        {
            this._CacheDictionaryStore = new CacheDictionaryStore<TKey, TValue, object>((key, _) => getFunction(key));
        }
        public TValue GetValue(TKey key)
        {
            return this._CacheDictionaryStore.GetValue(key, default);
        }

        public void ResetCache()
        {
            this._CacheDictionaryStore.ResetCache();
        }

        public void ResetCache(TKey item)
        {
            this._CacheDictionaryStore.ResetCache(item);
        }

        public bool ContainsKey(TKey key)
        {
            return this._CacheDictionaryStore.ContainsKey(key);
        }
    }
}