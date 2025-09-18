using HisaCat.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace HisaCat.HUE.Collections
{
    [Serializable]
    public class SerializedHashSet<T> :
        ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>,
        ISerializationCallbackReceiver
    {
        // Direct inheritance from HashSet causes persistent NullReferenceExceptions 
        // in components after script modifications, so we use composition instead.
        [SerializeField] private List<T> _items = null;
        [NonSerialized] private HashSet<T> hashSet = null;

        private ReadOnlyHashSet<T> _readOnlyHashSet = null;
        public IReadOnlyHashSet<T> ReadOnlyHashSet => this._readOnlyHashSet ??= this.hashSet.AsReadOnly();

        #region Constructors
        public SerializedHashSet()
        {
            this.hashSet = new();
            Initialize();
        }
        public SerializedHashSet(IEnumerable<T> collection)
        {
            this.hashSet = new(collection);
            Initialize();
        }
        public SerializedHashSet(IEqualityComparer<T> comparer)
        {
            this.hashSet = new(comparer);
            Initialize();
        }
        public SerializedHashSet(int capacity)
        {
            this.hashSet = new(capacity);
            Initialize();
        }
        public SerializedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            this.hashSet = new(collection, comparer);
            Initialize();
        }
        public SerializedHashSet(int capacity, IEqualityComparer<T> comparer)
        {
            this.hashSet = new(capacity, comparer);
            Initialize();
        }
        #endregion Constructors

        private void Initialize()
        {
            this._items = new(this.hashSet);
        }

        private HashSet<T> uniqueCache = new();
        public void ValidateList()
        {
            this.uniqueCache.Clear();
            this.uniqueCache.AddRange(this._items);

            this._items.Clear();
            foreach (var item in this.uniqueCache) this._items.Add(item);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this._items.Clear();
            foreach (var item in this.hashSet)
                this._items.Add(item);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.ValidateList();

            this.hashSet.Clear();
            foreach (var item in this._items)
                this.hashSet.Add(item);
        }

        #region Interface Implementations
        public int Count => this.hashSet.Count;
        public IEqualityComparer<T> Comparer => this.hashSet.Comparer;

        public static IEqualityComparer<HashSet<T>> CreateSetComparer() => HashSet<T>.CreateSetComparer();
        public bool Add(T item) => this.Add_Internal(item);
        public void Clear() => this.Clear_Internal();
        public bool Contains(T item) => this.hashSet.Contains(item);
        public void CopyTo(T[] array) => this.hashSet.CopyTo(array);
        public void CopyTo(T[] array, int arrayIndex) => this.hashSet.CopyTo(array, arrayIndex);
        public void CopyTo(T[] array, int arrayIndex, int count) => this.hashSet.CopyTo(array, arrayIndex, count);
        public int EnsureCapacity(int capacity) => this.hashSet.EnsureCapacity(capacity);
        public void ExceptWith(IEnumerable<T> other) => this.hashSet.ExceptWith(other);
        public HashSet<T>.Enumerator GetEnumerator() => this.hashSet.GetEnumerator();
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) => this.hashSet.GetObjectData(info, context);
        public void IntersectWith(IEnumerable<T> other) => this.hashSet.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.hashSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.hashSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this.hashSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.hashSet.IsSupersetOf(other);
        public virtual void OnDeserialization(object sender) => this.hashSet.OnDeserialization(sender);
        public bool Overlaps(IEnumerable<T> other) => this.hashSet.Overlaps(other);
        public bool Remove(T item) => this.Remove_Internal(item);
        public int RemoveWhere(Predicate<T> match) => this.RemoveWehere_Internal(match);
        public bool SetEquals(IEnumerable<T> other) => this.hashSet.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => this.hashSet.SymmetricExceptWith(other);
        public void TrimExcess() => this.hashSet.TrimExcess();
        public bool TryGetValue(T equalValue, out T actualValue) => this.hashSet.TryGetValue(equalValue, out actualValue);
        public void UnionWith(IEnumerable<T> other) => this.hashSet.UnionWith(other);

        public bool IsReadOnly => false;
        void ICollection<T>.Add(T item) => this.Add_Internal(item);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.hashSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.hashSet.GetEnumerator();
        #endregion Interface implementations

        private bool Add_Internal(T item)
        {
            var result = this.hashSet.Add(item);
            if (result) this._items.Add(item);
            return result;
        }
        private void Clear_Internal()
        {
            this.hashSet.Clear();
            this._items.Clear();
        }
        private bool Remove_Internal(T item)
        {
            var result = this.hashSet.Remove(item);
            if (result) this._items.Remove(item);
            return result;
        }
        private int RemoveWehere_Internal(Predicate<T> match)
        {
            var result = this.hashSet.RemoveWhere(match);
            if (result > 0) this._items.RemoveAll(match);
            return result;
        }
    }
}
