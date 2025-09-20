using System.Collections;
using System.Collections.Generic;

namespace HisaCat.HUE.Collections
{
    public class ReadOnlyHashSet<T> : IReadOnlyHashSet<T>
    {
        private const string READ_ONLY_ERROR_MESSAGE = "Collection is read-only";

        private readonly ISet<T> _data;

        public ReadOnlyHashSet(ISet<T> data)
        {
            _data = data;
        }

        #region ISet<T>
        bool ISet<T>.Add(T item) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this._data.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this._data.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this._data.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this._data.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this._data.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this._data.SetEquals(other);
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        void ISet<T>.UnionWith(IEnumerable<T> other) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        #endregion ISet<T>

        #region ICollection<T>
        public int Count => this._data.Count;
        public bool IsReadOnly => true;

        void ICollection<T>.Add(T item) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        void ICollection<T>.Clear() => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        public bool Contains(T item) => this._data.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => this._data.CopyTo(array, arrayIndex);
        bool ICollection<T>.Remove(T item) => throw new System.NotSupportedException(READ_ONLY_ERROR_MESSAGE);
        #endregion ICollection<T>

        #region IEnumerable<T>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._data.GetEnumerator();
        #endregion IEnumerable<T>

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator() => this._data.GetEnumerator();
        #endregion IEnumerable
    }

    public interface IReadOnlyHashSet<T> : IReadOnlyCollection<T>, ISet<T> { }

    public class ReadOnlyableHashSet<T> : HashSet<T>, IReadOnlyHashSet<T> { }

    public static class ReadOnlyHashSetExtensions
    {
        public static ReadOnlyHashSet<T> AsReadOnly<T>(this ISet<T> set)
            => new(set);
    }
}
