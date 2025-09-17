using HisaCat;
using System.Collections.Generic;
internal static class PhysicsCallbackCache
{
    internal interface ICacheContainer
    {
        void ClearAll();
    }

#if UNITY_EDITOR
#pragma warning disable IDE0051
    [UnityEditor.InitializeOnEnterPlayMode]
    private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
    {
        if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
        {
            foreach (var cache in cacheContainers)
                cache.ClearAll();
            cacheContainers.Clear();
        }
    }
#pragma warning restore IDE0051
#endif
    private static readonly List<ICacheContainer> cacheContainers = new();
    private static void RegisterCache(ICacheContainer cache) => cacheContainers.Add(cache);

    private sealed class StaticBuffers<TType> : ICacheContainer
    {
        private static readonly Dictionary<int, StaticBuffer<TType>> buffers = new();
        static StaticBuffers() => RegisterCache(new StaticBuffers<TType>());

        public StaticBuffer<TType> Get(int capacity)
        {
            if (buffers.TryGetValue(capacity, out var buffer) == false)
            {
                UnityEngine.Debug.Log($"New buffer created {typeof(TType).Name}:{capacity}");
                buffer = new StaticBuffer<TType>(_ => new TType[capacity]);
                buffers.Add(capacity, buffer);
            }
            return buffer;
        }

        void ICacheContainer.ClearAll()
        {
            foreach (var buffer in buffers.Values)
                buffer.ClearBuffer();
            buffers.Clear();
        }
    }
    private sealed class Dictionaries<TKey, TValue> : ICacheContainer
    {
        private static readonly Dictionary<int, Dictionary<TKey, TValue>> dictionaries = new();
        static Dictionaries() => RegisterCache(new Dictionaries<TKey, TValue>());

        public Dictionary<TKey, TValue> Get(int capacity)
        {
            if (dictionaries.TryGetValue(capacity, out var dict) == false)
            {
                UnityEngine.Debug.Log($"New dictionary created {typeof(TKey).Name}/{typeof(TValue).Name}:{capacity}");
                dict = new Dictionary<TKey, TValue>(capacity);
                dictionaries.Add(capacity, dict);
            }
            return dict;
        }

        void ICacheContainer.ClearAll()
        {
            foreach (var dict in dictionaries.Values)
                dict.Clear();
            dictionaries.Clear();
        }
    }

    public static StaticBuffer<T> GetStaticBuffer<T>(int capacity)
        => new StaticBuffers<T>().Get(capacity);
    public static Dictionary<TKey, TValue> GetStaticDictionary<TKey, TValue>(int capacity)
        => new Dictionaries<TKey, TValue>().Get(capacity);
}
