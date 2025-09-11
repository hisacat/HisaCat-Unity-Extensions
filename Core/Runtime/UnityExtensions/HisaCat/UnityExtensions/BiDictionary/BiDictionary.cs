using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace HisaCat.Collections
{
    public class BiDictionary<TFirst, TSecond> :
        IEnumerable<KeyValuePair<TFirst, TSecond>>,
        IEnumerable,
        IReadOnlyCollection<KeyValuePair<TFirst, TSecond>>,
        IReadonlyBiDictionary<TFirst, TSecond>
    {
        private readonly IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        private readonly IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public TSecond this[TFirst first] => firstToSecond[first];
        public TFirst this[TSecond second] => secondToFirst[second];

        public IEnumerable<TFirst> Firsts => firstToSecond.Keys;
        public IEnumerable<TSecond> Seconds => secondToFirst.Keys;

        public bool ContainsFirst(TFirst first) => this.firstToSecond.ContainsKey(first);
        public bool ContainsSecond(TSecond second) => this.secondToFirst.ContainsKey(second);

        public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator() => this.firstToSecond.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => this.firstToSecond.Count;
        public void Clear()
        {
            this.firstToSecond.Clear();
            this.secondToFirst.Clear();
        }

        #region Exception throwing methods
        public void Add(TFirst first, TSecond second)
        {
            if (this.firstToSecond.ContainsKey(first) || this.secondToFirst.ContainsKey(second))
                throw new ArgumentException("Duplicate first or second");

            this.firstToSecond.Add(first, second);
            this.secondToFirst.Add(second, first);
        }

        public TSecond GetByFirst(TFirst first)
        {
            if (this.firstToSecond.TryGetValue(first, out var second) == false)
                throw new ArgumentException("first");

            return second;
        }
        public TFirst GetBySecond(TSecond second)
        {
            if (this.secondToFirst.TryGetValue(second, out var first) == false)
                throw new ArgumentException("second");

            return first;
        }

        public void RemoveByFirst(TFirst first)
        {
            if (this.firstToSecond.TryGetValue(first, out var second) == false)
                throw new ArgumentException("first");

            this.firstToSecond.Remove(first);
            this.secondToFirst.Remove(second);
        }

        public void RemoveBySecond(TSecond second)
        {
            if (this.secondToFirst.TryGetValue(second, out var first) == false)
                throw new ArgumentException("second");

            this.secondToFirst.Remove(second);
            this.firstToSecond.Remove(first);
        }
        #endregion Exception throwing methods

        #region Try methods
        public bool TryAdd(TFirst first, TSecond second)
        {
            if (this.firstToSecond.ContainsKey(first) || this.secondToFirst.ContainsKey(second))
                return false;

            this.firstToSecond.Add(first, second);
            this.secondToFirst.Add(second, first);
            return true;
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return this.firstToSecond.TryGetValue(first, out second);
        }
        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return this.secondToFirst.TryGetValue(second, out first);
        }

        public bool TryRemoveByFirst(TFirst first)
        {
            if (this.firstToSecond.TryGetValue(first, out var second) == false)
                return false;

            this.firstToSecond.Remove(first);
            this.secondToFirst.Remove(second);
            return true;
        }
        public bool TryRemoveBySecond(TSecond second)
        {
            if (this.secondToFirst.TryGetValue(second, out var first) == false)
                return false;

            this.secondToFirst.Remove(second);
            this.firstToSecond.Remove(first);
            return true;
        }

        public void Add(KeyValuePair<TFirst, TSecond> item)
        {
            this.firstToSecond.Add(item);
            this.secondToFirst.Add(new(item.Value, item.Key));
        }

        public bool Contains(KeyValuePair<TFirst, TSecond> item)
        {
            return this.firstToSecond.Contains(item);
        }

        public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TFirst, TSecond> item)
        {
            throw new NotImplementedException();
        }
        #endregion Try methods
    }

    public interface IReadonlyBiDictionary<TFirst, TSecond> :
        IEnumerable<KeyValuePair<TFirst, TSecond>>,
        IEnumerable,
        IReadOnlyCollection<KeyValuePair<TFirst, TSecond>>
    {
        TSecond this[TFirst first] { get; }
        TFirst this[TSecond second] { get; }

        IEnumerable<TFirst> Firsts { get; }
        IEnumerable<TSecond> Seconds { get; }

        bool ContainsFirst(TFirst first);
        bool ContainsSecond(TSecond second);
    }
}
