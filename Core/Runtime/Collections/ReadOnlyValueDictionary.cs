using System.Collections;
using System.Collections.Generic;

namespace HisaCat.HUE.Collections
{
    public abstract class ReadOnlyValueDictionary<K, V, RV> : IEnumerable<KeyValuePair<K, V>>
    {
        public abstract RV ToReadOnlyValue(V value);

        public Dictionary<K, RV> ReadOnlyDictionary { get; private set; } = null;
        private Dictionary<K, V> sourceDictionary = null;

        public ReadOnlyValueDictionary()
        {
            this.sourceDictionary = new();
            this.ReadOnlyDictionary = new();
        }

        #region IEnumerable
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            => this.sourceDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion

        #region Dictionary wrapper
        public V this[K key]
        {
            get => this.sourceDictionary[key];
            set
            {
                this.sourceDictionary[key] = value;
                this.ReadOnlyDictionary[key] = this.ToReadOnlyValue(value);
            }
        }
        public void Add(K key, V value)
        {
            this.sourceDictionary.Add(key, value);
            this.ReadOnlyDictionary.Add(key, this.ToReadOnlyValue(value));
        }
        public void Remove(K key)
        {
            this.sourceDictionary.Remove(key);
            this.ReadOnlyDictionary.Remove(key);
        }
        public void Clear()
        {
            this.sourceDictionary.Clear();
            this.ReadOnlyDictionary.Clear();
        }
        public bool ContainsKey(K key)
            => this.sourceDictionary.ContainsKey(key);
        public bool TryGetValue(K key, out V value)
            => this.sourceDictionary.TryGetValue(key, out value);
        public IEnumerable<K> Keys
            => this.sourceDictionary.Keys;
        public IEnumerable<V> Values
            => this.sourceDictionary.Values;
        public int Count
            => this.sourceDictionary.Count;
        #endregion Dictionary wrapper
    }
}
