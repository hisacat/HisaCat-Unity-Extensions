#if HUE_UNITY_ADDRESSABLES
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

using UnityAddressables = UnityEngine.AddressableAssets.Addressables;

namespace HisaCat.HUE.Assets
{
    public static partial class AssetLoader
    {
        public static class Addressables
        {
            public static bool KeyExists(string key, System.Type type)
            {
                var op = UnityAddressables.LoadResourceLocationsAsync(key, type);
                var locations = op.WaitForCompletion();
                op.Release();
                return locations.Count > 0;
            }
            public static AsyncOperationHandle<IList<IResourceLocation>> KeyExistsAsync(string key, System.Type type)
            {
                var locationOp = UnityAddressables.LoadResourceLocationsAsync(key, type);
                return locationOp;
            }
            public static AsyncOperationHandle<IList<IResourceLocation>> KeyExistsAsync(string key, System.Type type, System.Action<bool> callback)
            {
                var locationOp = KeyExistsAsync(key, type);
                locationOp.Completed += (op) =>
                {
                    bool exists = op.Result.Count > 0;
                    callback.Invoke(exists);
                    op.Release();
                };
                return locationOp;
            }

            private static bool IsComponentType<T>() where T : Object
                => typeof(T).IsSubclassOf(typeof(Component)) || typeof(T) == typeof(Component);

            public static T LoadSync<T>(string key) where T : Object
                => IsComponentType<T>() ? LoadPrefabSync<T>(key) : LoadObjectSync<T>(key);
            private static T LoadPrefabSync<T>(string key) where T : Object
            {
                if (KeyExists(key, typeof(GameObject)) == false) return null;
                var op = UnityAddressables.LoadAssetAsync<GameObject>(key);
                var gameObject = op.WaitForCompletion();
                if (gameObject.TryGetComponent<T>(out var component) == false)
                {
                    Debug.LogWarning($"{nameof(Addressables)}: Component '{typeof(T).Name}' not found in GameObject '{gameObject.name}'.");
                    return null;
                }
                return component;
            }
            private static T LoadObjectSync<T>(string key) where T : Object
            {
                if (KeyExists(key, typeof(T)) == false) return null;
                var op = UnityAddressables.LoadAssetAsync<T>(key);
                T asset = op.WaitForCompletion();
                return asset;
            }

            public static AsyncOperationHandle<T> LoadAsync<T>(string key) where T : Object
                => IsComponentType<T>() ? LoadPrefabAsync<T>(key) : LoadObjectAsync<T>(key);
            private static AsyncOperationHandle<T> LoadPrefabAsync<T>(string key) where T : Object
            {
                var checkOp = KeyExistsAsync(key, typeof(GameObject));
                return UnityAddressables.ResourceManager.CreateChainOperation(checkOp, handleGameObjectLoadForComponent);
                AsyncOperationHandle<T> handleGameObjectLoadForComponent(AsyncOperationHandle<IList<IResourceLocation>> keyCheckOp)
                {
                    if (keyCheckOp.Result.Count <= 0) return UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);

                    var gameObjectOp = UnityAddressables.LoadAssetAsync<GameObject>(key);
                    return UnityAddressables.ResourceManager.CreateChainOperation(gameObjectOp, handleComponentExtraction);
                    AsyncOperationHandle<T> handleComponentExtraction(AsyncOperationHandle<GameObject> gameObjectOp)
                    {
                        var gameObject = gameObjectOp.Result;
                        if (gameObject.TryGetComponent<T>(out var component) == false)
                        {
                            Debug.LogWarning($"{nameof(Addressables)}: Component '{typeof(T).Name}' not found in GameObject '{gameObject.name}'.");
                            return UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);
                        }
                        return UnityAddressables.ResourceManager.CreateCompletedOperation<T>(component, null);
                    }
                }
            }
            private static AsyncOperationHandle<T> LoadObjectAsync<T>(string key) where T : Object
            {
                var checkOp = KeyExistsAsync(key, typeof(T));
                return UnityAddressables.ResourceManager.CreateChainOperation(checkOp, handleKeyCheckResult);
                AsyncOperationHandle<T> handleKeyCheckResult(AsyncOperationHandle<IList<IResourceLocation>> keyCheckOp)
                {
                    if (keyCheckOp.Result.Count <= 0) return UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);

                    return UnityAddressables.LoadAssetAsync<T>(key);
                }
            }

            public static AsyncOperationHandle<T> LoadAsync<T>(string key, System.Action<T> completed) where T : Object
            {
                var op = LoadAsync<T>(key);
                op.Completed += onCompleted;
                void onCompleted(AsyncOperationHandle<T> completedOp)
                {
                    completed.Invoke(completedOp.Result);
                    op.Completed -= onCompleted;
                }
                return op;
            }

            public static AsyncOperationHandle<IList<T>> LoadManyOrderedAsync<T>(IList<string> keys) where T : Object
                => IsComponentType<T>() ? LoadManyPrefabOrderedAsync<T>(keys) : LoadManyObjectOrderedAsync<T>(keys);

            private static AsyncOperationHandle<IList<T>> LoadManyPrefabOrderedAsync<T>(IList<string> keys) where T : Object
            {
                var handles = new List<AsyncOperationHandle<T>>();

                foreach (var key in keys)
                {
                    AsyncOperationHandle<T> handle;

                    if (KeyExists(key, typeof(GameObject)) == false)
                    {
                        handle = UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);
                    }
                    else
                    {
                        var goHandle = UnityAddressables.LoadAssetAsync<GameObject>(key);
                        handle = UnityAddressables.ResourceManager.CreateChainOperation(goHandle, (goOp) =>
                        {
                            T component = null;
                            if (goOp.Result != null) component = goOp.Result.GetComponent<T>();
                            return UnityAddressables.ResourceManager.CreateCompletedOperation(component, null);
                        });
                    }

                    handles.Add(handle);
                }

                return CreateGroupOperation(handles);
            }

            private static AsyncOperationHandle<IList<T>> LoadManyObjectOrderedAsync<T>(IList<string> keys) where T : Object
            {
                var handles = new List<AsyncOperationHandle<T>>();

                foreach (var key in keys)
                {
                    AsyncOperationHandle<T> handle;

                    if (KeyExists(key, typeof(T)) == false)
                        handle = UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);
                    else
                        handle = UnityAddressables.LoadAssetAsync<T>(key);

                    handles.Add(handle);
                }

                return CreateGroupOperation(handles);
            }
            private static AsyncOperationHandle<IList<T>> CreateGroupOperation<T>(List<AsyncOperationHandle<T>> handles) where T : Object
            {
                var _handles = new List<AsyncOperationHandle>();
                foreach (var handle in handles) _handles.Add(handle);
                var groupOp = UnityAddressables.ResourceManager.CreateGenericGroupOperation(_handles);

                return UnityAddressables.ResourceManager.CreateChainOperation(groupOp, (groupResult) =>
                {
                    var results = new List<T>();
                    for (int i = 0; i < handles.Count; i++) results.Add(handles[i].Result);
                    return UnityAddressables.ResourceManager.CreateCompletedOperation<IList<T>>(results, null);
                });
            }

            public static AsyncOperationHandle<IList<T>> LoadManyOrderedAsync<T>(IList<string> keys, System.Action<IList<T>> completed) where T : Object
            {
                var op = LoadManyOrderedAsync<T>(keys);
                op.Completed += onCompleted;
                void onCompleted(AsyncOperationHandle<IList<T>> completedOp)
                {
                    completed.Invoke(completedOp.Result);
                    op.Completed -= onCompleted;
                }
                return op;
            }
        }
    }
}
#endif
