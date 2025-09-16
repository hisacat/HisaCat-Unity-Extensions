using System.Collections;
using System.Collections.Generic;

namespace HisaCat.HUE.Collections
{
    public class ReadOnlyHashSet<T> : IReadOnlyHashSet<T>
    {
        readonly ISet<T> _data;

        public ReadOnlyHashSet(ISet<T> data)
        {
            _data = data;
        }

        public int Count
            => this._data.Count;

        public bool Contains(T item)
            => this._data.Contains(item);

        public IEnumerator<T> GetEnumerator()
            => this._data.GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other)
            => this._data.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other)
            => this._data.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other)
            => this._data.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other)
            => this._data.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other)
            => this._data.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other)
            => this._data.SetEquals(other);

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }

    public interface IReadOnlyHashSet<T> : IReadOnlyCollection<T>
    {
        // ICollection
        bool Contains(T item);
        // ISet
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
    }

    public class ReadOnlyableHashSet<T> : HashSet<T>, IReadOnlyHashSet<T>
    {
    }

    public static class ReadOnlyHashSetExtensions
    {
        public static IReadOnlyHashSet<T> AsReadOnly<T>(this ISet<T> set)
            => new ReadOnlyHashSet<T>(set);
    }
}
