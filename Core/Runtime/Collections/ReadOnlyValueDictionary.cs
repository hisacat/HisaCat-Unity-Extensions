using System.Collections;
using System.Collections.Generic;

namespace HisaCat.HUE.Collections
{
    public abstract class ReadOnlyValueDictionary<TKey, TValue, TReadOnlyValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public abstract TReadOnlyValue ToReadOnlyValue(TValue value);

        public Dictionary<TKey, TReadOnlyValue> ReadOnly { get; private set; } = null;
        private readonly Dictionary<TKey, TValue> sourceDictionary = null;

        public ReadOnlyValueDictionary()
        {
            this.sourceDictionary = new();
            this.ReadOnly = new();
        }

        #region IEnumerable
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => this.sourceDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion

        #region Dictionary wrapper
        public TValue this[TKey key]
        {
            get => this.sourceDictionary[key];
            set
            {
                this.sourceDictionary[key] = value;
                this.ReadOnly[key] = this.ToReadOnlyValue(value);
            }
        }
        public void Add(TKey key, TValue value)
        {
            this.sourceDictionary.Add(key, value);
            this.ReadOnly.Add(key, this.ToReadOnlyValue(value));
        }
        public void Remove(TKey key)
        {
            this.sourceDictionary.Remove(key);
            this.ReadOnly.Remove(key);
        }
        public void Clear()
        {
            this.sourceDictionary.Clear();
            this.ReadOnly.Clear();
        }
        public bool ContainsKey(TKey key)
            => this.sourceDictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value)
            => this.sourceDictionary.TryGetValue(key, out value);
        public IEnumerable<TKey> Keys
            => this.sourceDictionary.Keys;
        public IEnumerable<TValue> Values
            => this.sourceDictionary.Values;
        public int Count
            => this.sourceDictionary.Count;
        #endregion Dictionary wrapper
    }
}
