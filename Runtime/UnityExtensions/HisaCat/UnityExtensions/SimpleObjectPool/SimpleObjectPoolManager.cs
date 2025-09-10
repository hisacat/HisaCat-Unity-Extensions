using HisaCat.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HisaCat.SimpleObjectPool
{
    public class SimpleObjectPoolManager : MonoBehaviour
    {
        #region Initialize static fields for DisableDomainReload on EnterPlaymodeInEditor
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                IsApplicationQuitting = false;
                _instance = null;

                Application.quitting -= Application_quitting;
                Application.quitting += Application_quitting;
                static void Application_quitting() => IsApplicationQuitting = true;
            }
        }
#pragma warning restore IDE0051
#endif
        #endregion

        private static bool IsApplicationQuitting = false;
        private static SimpleObjectPoolManager _instance = null;
        private static SimpleObjectPoolManager instance
        {
            get
            {
                if (_instance == null && IsApplicationQuitting == false)
                {
                    var go = new GameObject($"[HisaCat.{nameof(SimpleObjectPoolManager)}]");
                    _instance = go.AddComponent<SimpleObjectPoolManager>();
                    _instance.Initialize();
                    GameObject.DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
        }
        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private class ObjectPool
        {
            public readonly MonoBehaviour Origin = null;
            public readonly int UID = 0;

            public int TotalCount => this.SpawnedCount + this.SpawnableCount;
            public int SpawnedCount => this.SpawnedPool.Count;
            public int SpawnableCount => this.SpawnablePool.Count;

            public readonly SimpleLinkedList<ObjectPoolInstance> SpawnedPool = null;
            public readonly SimpleLinkedList<ObjectPoolInstance> SpawnablePool = null;

            public ObjectPool(MonoBehaviour origin, int uid)
            {
                this.Origin = origin;
                this.UID = uid;

                this.SpawnedPool = new SimpleLinkedList<ObjectPoolInstance>();
                this.SpawnablePool = new SimpleLinkedList<ObjectPoolInstance>();
            }

            public void AddPoolInstance<T>(T target) where T : MonoBehaviour, ISimpleObjectPoolable
            {
                var instance = new ObjectPoolInstance(this, target, target);

                instance.SetNode(this.SpawnablePool.AddFirst(instance));

                SimpleObjectPoolManager.instance.objectPoolInstanceCacheDic.Add(instance.iInstance, instance);
            }
            public void RemovePoolInstance(ObjectPoolInstance instance)
            {
                SimpleObjectPoolManager.instance.objectPoolInstanceCacheDic.Remove(instance.iInstance);

                instance.Node.List.Remove(instance.Node);
            }
            private void OnPoolInstanceSpawnCallback(ObjectPoolInstance instance)
            {
                instance.Node.List.Remove(instance.Node);
                this.SpawnedPool.AddFirst(instance.Node);
            }
            private void OnPoolInstanceDespawnCallback(ObjectPoolInstance instance)
            {
                instance.Node.List.Remove(instance.Node);
                this.SpawnablePool.AddFirst(instance.Node);
            }
            public class ObjectPoolInstance
            {
                public readonly ObjectPool ParentPool = null;

                public bool isActivated = false; //IsSpawned
                public readonly MonoBehaviour instance = null;
                public readonly ISimpleObjectPoolable iInstance = null;

                public SimpleLinkedList<ObjectPoolInstance>.Node Node { get; private set; }

                public ObjectPoolInstance(ObjectPool parent, MonoBehaviour instance, ISimpleObjectPoolable iInstance)
                {
                    this.ParentPool = parent;
                    this.isActivated = false;
                    this.instance = instance;
                    this.iInstance = iInstance;
                }
                public void SetNode(SimpleLinkedList<ObjectPoolInstance>.Node node) => this.Node = node;

                public bool IsValid() => this.instance != null && this.iInstance != null;

                public void Spawn()
                {
                    if (this.isActivated)
                    {
                        Debug.LogError($"[{nameof(SimpleObjectPoolManager)}] {nameof(ObjectPoolInstance)}.{nameof(Spawn)}: instance already spawned!", this.instance);
                        return;
                    }

                    this.isActivated = true;
                    this.instance.gameObject.SetActive(true);
                    this.ParentPool.OnPoolInstanceSpawnCallback(this);

                    this.iInstance.OnSpawnFromPool();
                }
                public void Despawn()
                {
                    if (this.isActivated == false)
                    {
                        Debug.LogError($"[{nameof(SimpleObjectPoolManager)}] {nameof(ObjectPoolInstance)}.{nameof(Despawn)}: instance dose not spawned!", this.instance);
                        return;
                    }

                    this.isActivated = false;
                    this.instance.gameObject.SetActive(false);
                    this.ParentPool.OnPoolInstanceDespawnCallback(this);

                    this.iInstance.OnDespawnFromPool();
                }
            }
        }

        private Dictionary<MonoBehaviour, Dictionary<int, ObjectPool>> objectPoolDic; // <Prefab, <UID, Pool>>
        private Dictionary<ISimpleObjectPoolable, ObjectPool.ObjectPoolInstance> objectPoolInstanceCacheDic = null;

        private void Initialize()
        {
            this.objectPoolDic = new();
            this.objectPoolInstanceCacheDic = new();
        }

        /// <summary>
        /// Add object pool instances manually.
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="origin">Original instance (in normal case it might be prefab)</param>
        /// <param name="count">Pool count</param>
        public static void AddPool<T>(T origin, int count) where T : MonoBehaviour, ISimpleObjectPoolable
            => AddPool(origin, origin.GetHashCode(), count);
        public static void AddPool<T>(T origin, string poolName, int count) where T : MonoBehaviour, ISimpleObjectPoolable
            => AddPool(origin, poolName.GetHashCode(), count);
        private static void AddPool<T>(T origin, int uid, int count) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            if (instance.objectPoolDic.ContainsKey(origin) == false)
                instance.objectPoolDic.Add(origin, new());

            if (instance.objectPoolDic[origin].ContainsKey(uid) == false)
                instance.objectPoolDic[origin].Add(uid, new ObjectPool(origin, uid));

            var objectPool = instance.objectPoolDic[origin][uid];

            var wasTargetActive = origin.gameObject.activeSelf;
            origin.gameObject.SetActive(false);
            {
                for (int i = 0; i < count; i++)
                {
                    var newInstance = Instantiate(origin);
                    //newInstance.gameObject.SetActive(false);
                    newInstance.transform.SetParent(instance.transform);
                    objectPool.AddPoolInstance(newInstance);
                }
            }
            origin.gameObject.SetActive(wasTargetActive);
        }

        /// <summary>
        /// Get pool count
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="target">Original instance (in normal case it might be prefab)</param>
        /// <returns></returns>
        public static int GetPoolCount<T>(T target) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetPoolCount(target, target.GetHashCode());
        public static int GetPoolCount<T>(T target, string poolName) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetPoolCount(target, poolName.GetHashCode());
        private static int GetPoolCount<T>(T target, int uid) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            var type = typeof(T);

            if (instance.objectPoolDic.ContainsKey(target) == false || instance.objectPoolDic[target].ContainsKey(uid) == false)
                return 0;

            return instance.objectPoolDic[target][uid].TotalCount;
        }

        /// <summary>
        /// Get spawnable pool count
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="target">Original instance (in normal case it might be prefab)</param>
        /// <returns></returns>
        public static int GetSpawnablePoolCount<T>(T target) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetSpawnablePoolCount(target, target.GetHashCode());
        public static int GetSpawnablePoolCount<T>(T target, string poolName) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetSpawnablePoolCount(target, poolName.GetHashCode());
        private static int GetSpawnablePoolCount<T>(T target, int uid) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            var type = typeof(T);

            if (instance.objectPoolDic.ContainsKey(target) == false || instance.objectPoolDic[target].ContainsKey(uid) == false)
                return 0;

            return instance.objectPoolDic[target][uid].SpawnableCount;
        }

        /// <summary>
        /// Get pool count and spawnable pool count
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Special name (uid)(</param>
        /// <param name="target">Original instance (in normal case it might be prefab)</param>
        /// <param name="poolCount">Pool count</param>
        /// <param name="spawnableCount">Spawnable pool count</param>
        public static void GetPoolCountDetail<T>(T target, out int poolCount, out int spawnableCount) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetPoolCountDetail(target, target.GetHashCode(), out poolCount, out spawnableCount);
        public static void GetPoolCountDetail<T>(T target, string poolName, out int poolCount, out int spawnableCount) where T : MonoBehaviour, ISimpleObjectPoolable
            => GetPoolCountDetail(target, poolName.GetHashCode(), out poolCount, out spawnableCount);
        private static void GetPoolCountDetail<T>(T target, int uid, out int poolCount, out int spawnableCount) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            var type = typeof(T);

            if (instance.objectPoolDic.ContainsKey(target) == false || instance.objectPoolDic[target].ContainsKey(uid) == false)
            {
                poolCount = 0;
                spawnableCount = 0;
                return;
            }

            poolCount = instance.objectPoolDic[target][uid].TotalCount;
            spawnableCount = instance.objectPoolDic[target][uid].SpawnableCount;
            return;
        }

        /// <summary>
        /// Spawn object from pool instance. (If instance not exist, create new one)
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="target">Original instance (in normal case it might be prefab)</param>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Parent</param>
        /// <returns></returns>
        public static T Spawn<T>(T target, Vector3 position, Quaternion rotation, Transform parent = null) where T : MonoBehaviour, ISimpleObjectPoolable
            => Spawn(target, target.GetHashCode(), position, rotation, parent);
        public static T Spawn<T>(T target, string poolName, Vector3 position, Quaternion rotation, Transform parent = null) where T : MonoBehaviour, ISimpleObjectPoolable
            => Spawn(target, poolName.GetHashCode(), position, rotation, parent);
        private static T Spawn<T>(T target, int uid, Vector3 position, Quaternion rotation, Transform parent = null) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            if (parent == null) parent = instance.transform;
            var type = typeof(T);

            if (instance.objectPoolDic.ContainsKey(target) && instance.objectPoolDic[target].ContainsKey(uid))
            {
                var objectPool = instance.objectPoolDic[target][uid];
                //instance.ValidateObjectPool(objectPool);

                var poolInstance = objectPool.SpawnablePool.Count <= 0 ? null : objectPool.SpawnablePool.First.Value;
                if (poolInstance != null)
                {
                    if (poolInstance.IsValid() == false)
                    {
                        //ObjectPool 인스턴스를 RemoveFromPool의 호출 없이 삭제했을 경우의 안내 메세지.
                        //씬 전환, 명시적/실수로 인한 Destroy를 위해선, RemoveFromPool를 호출해야한다. (OnDestroy에서 RemoveFromPool 호출)
                        // * RemoveFromPool를 호출하지 않아도 로직상으로 문제가 없도록 설계되었으나, RemoveFromPool를 호출하는것이 Performance측면에서 유리함.
                        // * 로직상 문제가 없도록 설계된 로직: 여기서 missing이 감지되면, 해당 instance reference를 pool list에서 제거하고, Spawn을 재귀로 재호출한다.
                        Debug.LogError($"[{nameof(SimpleObjectPoolManager)}] {nameof(Spawn)}: ObjectPool instance is <color=red>missing or destroyed!</color> <b>(more info messages...)</b>" +
                            "\r\nMake sure if <b>this instance <color=red>destroy</color> manually</b>, call <b><color=green>\"RemoveFromPool\"</color></b> function." +
                            "\r\nIt will be remove from pool automatically, but destroy pool instance without <b><color=green>\"RemoveFromPool\"</color></b> function wiil be get <b>bad performance.</b>" +
                            $"\r\nCurrent type: <b>{type.Name}</b> ({type.FullName})" +
                            "\r\n");

                        objectPool.RemovePoolInstance(poolInstance);
                        return Spawn(target, uid, position, rotation, parent);
                    }

                    poolInstance.instance.transform.SetParent(parent);
                    poolInstance.instance.transform.SetPositionAndRotation(position, rotation);
                    poolInstance.instance.transform.localScale = Vector3.one;
                    poolInstance.Spawn();

                    return poolInstance.instance as T;
                }
            }

            //if useable pool instance not exist, make new one.
            {
                AddPool(target, uid, 1);

                //For better performance:
                var poolInstance = instance.objectPoolDic[target][uid].SpawnablePool.First.Value;

                poolInstance.instance.transform.SetParent(parent);
                poolInstance.instance.transform.SetPositionAndRotation(position, rotation);
                poolInstance.Spawn();
                return poolInstance.instance as T;
            }
        }


        /// <summary>
        /// Deactiate and return target to pool instance. if object not in pool, it will destroy target.
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="target">Instance</param>
        public static void Despawn<T>(T target) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            if (IsObjectPoolInstance(target))
            {
                var poolInstance = instance.objectPoolInstanceCacheDic[target];
                poolInstance.Despawn();

                poolInstance.instance.transform.SetParent(instance.transform);
            }
            else
            {
                Debug.LogError("[SimpleObjectPoolManager]Despawn: Cannot find pool from cache. do destroy", target);
                Destroy(target.gameObject);
            }
        }
        public static bool IsObjectPoolInstance<T>(T target) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            return instance.objectPoolInstanceCacheDic.ContainsKey(target);
        }

        /// <summary>
        /// Remove(Detach) pool instance. In normal case It should be call under "OnDestroy" if component is using Object pool. (overrided ISimpleObjectPoolable)
        /// </summary>
        /// <typeparam name="T">Target type(ISimpleObjectPoolable component)</typeparam>
        /// <param name="target">Remove target instance</param>
        public static bool RemoveFromPool<T>(T target, bool printWarning = true) where T : MonoBehaviour, ISimpleObjectPoolable
        {
            if (instance == null) return false;

            if (instance.objectPoolInstanceCacheDic.ContainsKey(target))
            {
                var poolInstance = instance.objectPoolInstanceCacheDic[target];
                var objectPool = poolInstance.ParentPool;
                objectPool.RemovePoolInstance(poolInstance);

                //Clean-up empty pool.
                if (objectPool.TotalCount <= 0)
                {
                    if (instance.objectPoolDic.ContainsKey(objectPool.Origin) == false)
                    {
                        Debug.LogError($"[SimpleObjectPoolManager]RemoveFromPool: Trying to clean-up empty pool but current instance's Origin \"{objectPool.Origin.name}\" not exist!", target);
                        return false;
                    }

                    if (instance.objectPoolDic[objectPool.Origin].ContainsKey(objectPool.UID) == false)
                    {
                        Debug.LogError($"[SimpleObjectPoolManager]RemoveFromPool: Trying to clean-up empty pool but current instance's UID \"{objectPool.UID}\" not exist!", target);
                        return false;
                    }

                    instance.objectPoolDic[objectPool.Origin].Remove(objectPool.UID);
                    if (instance.objectPoolDic[objectPool.Origin].Count <= 0)
                    {
                        instance.objectPoolDic.Remove(objectPool.Origin);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Destroy specific object pool and instances.
        /// </summary>
        /// <param name="origin">origin object for Target pool.</param>
        public static void DestroyPool<T>(T origin) where T : MonoBehaviour
            => DestroyPool(origin, origin.GetHashCode());
        public static void DestroyPool<T>(T origin, string poolName) where T : MonoBehaviour
            => DestroyPool(origin, poolName.GetHashCode());
        private static void DestroyPool<T>(T origin, int uid) where T : MonoBehaviour
        {
            if (instance.objectPoolDic.ContainsKey(origin) == false)
                return;

            var poolByUID = instance.objectPoolDic[origin];
            if (poolByUID.ContainsKey(uid) == false)
                return;

            var objectPool = poolByUID[uid];

            //Despawn all spawned instances first.
            {
                var head = objectPool.SpawnedPool.First;
                while (head != null)
                {
                    var temp = head;
                    head = head.Next;

                    temp.Value.Despawn();
                }
                /*
                var count = objectPool.SpawnedPool.Count;
                for (int i = count - 1; i >= 0; i--)
                    objectPool.SpawnedPool[count - 1].Despawn();
                */
            }

            //Remove and destroy all spawnable(despawn) instances.
            {
                var head = objectPool.SpawnedPool.First;
                while (head != null)
                {
                    var temp = head;
                    head = head.Next;

                    objectPool.RemovePoolInstance(temp.Value);
                    Destroy(temp.Value.instance.gameObject);
                }
            }

            //Clean-up empty pool.
            if (poolByUID.Remove(uid))
            {
                if (poolByUID.Count <= 0)
                {
                    instance.objectPoolDic.Remove(origin);
                }
            }
        }

        /// <summary>
        /// Destroy all object pool and instances.
        /// </summary>
        public static void DestroyPoolAll()
        {
            var names = instance.objectPoolDic.Keys.ToList();
            int nameCount = names.Count;
            for (int i = 0; i < nameCount; i++)
            {
                var name = names[i];
                var types = instance.objectPoolDic[name].Keys.ToList();
                int typeCount = types.Count;
                for (int j = 0; j < typeCount; j++)
                {
                    var type = types[i];
                    DestroyPool(name, type);
                }
            }
        }

        /// <summary>
        /// Remove invalid object pool instances like Destroyed, Missing, etc...
        /// </summary>
        public void ValidateObjectPool()
        {
            using (var nameEnumerator = instance.objectPoolDic.Keys.GetEnumerator())
            {
                while (nameEnumerator.MoveNext())
                {
                    var poolByType = instance.objectPoolDic[nameEnumerator.Current];
                    using (var typeEnumerator = poolByType.Values.GetEnumerator())
                    {
                        while (typeEnumerator.MoveNext())
                        {
                            var pool = typeEnumerator.Current;
                            ValidateObjectPool(pool);
                        }
                    }
                }
            }
        }
        private void ValidateObjectPool(ObjectPool pool)
        {
            {
                var head = pool.SpawnedPool.First;
                while (head != null)
                {
                    var temp = head;
                    head = head.Next;

                    if (temp.Value.IsValid() == false)
                        pool.RemovePoolInstance(temp.Value);
                }
            }
            {
                var head = pool.SpawnablePool.First;
                while (head != null)
                {
                    var temp = head;
                    head = head.Next;

                    if (temp.Value.IsValid() == false)
                        pool.RemovePoolInstance(temp.Value);
                }
            }
        }
    }
}
