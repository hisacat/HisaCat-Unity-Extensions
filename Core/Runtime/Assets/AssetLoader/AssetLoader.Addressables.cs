#if HUE_UNITY_ADDRESSABLES
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityAddressables = UnityEngine.AddressableAssets.Addressables;

namespace HisaCat.HUE.Assets
{
    public static partial class AssetLoader
    {
        public static class Addressables
        {
#if !UNITY_WEBGL // WebGL does not support 'WaitForCompletion'
            public static bool KeyExists(string key, System.Type type)
            {
                var op = UnityAddressables.LoadResourceLocationsAsync(key, type);
                var locations = op.WaitForCompletion();
                op.Release();
                return locations.Count > 0;
            }
#endif
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

#if !UNITY_WEBGL // WebGL does not support 'WaitForCompletion'
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
#endif

#if !UNITY_WEBGL // WebGL does not support 'WaitForCompletion'
            public static SceneInstance LoadSceneSync(string key)
                => WaitAsyncOp(() => UnityAddressables.LoadSceneAsync(key));
            public static SceneInstance LoadSceneSync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded)
                => WaitAsyncOp(() => UnityAddressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority, releaseMode));
            public static SceneInstance LoadSceneSync(object key, LoadSceneMode loadMode, SceneReleaseMode releaseMode, bool activateOnLoad = true, int priority = 100)
                => WaitAsyncOp(() => UnityAddressables.LoadSceneAsync(key, loadMode, releaseMode, activateOnLoad, priority));
            public static SceneInstance LoadSceneSync(object key, LoadSceneParameters loadSceneParameters, bool activateOnLoad = true, int priority = 100)
                => WaitAsyncOp(() => UnityAddressables.LoadSceneAsync(key, loadSceneParameters, activateOnLoad, priority));
            public static SceneInstance LoadSceneSync(object key, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode, bool activateOnLoad = true, int priority = 100)
                => WaitAsyncOp(() => UnityAddressables.LoadSceneAsync(key, loadSceneParameters, releaseMode, activateOnLoad, priority));
            private static T WaitAsyncOp<T>(System.Func<AsyncOperationHandle<T>> createOp)
            {
                var op = createOp.Invoke();
                op.WaitForCompletion();
                return op.Result;
            }
#endif

            public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string key)
                => UnityAddressables.LoadSceneAsync(key);
            public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, SceneReleaseMode releaseMode = SceneReleaseMode.ReleaseSceneWhenSceneUnloaded)
                => UnityAddressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority, releaseMode);
            public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode, SceneReleaseMode releaseMode, bool activateOnLoad = true, int priority = 100)
                => UnityAddressables.LoadSceneAsync(key, loadMode, releaseMode, activateOnLoad, priority);
            public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneParameters loadSceneParameters, bool activateOnLoad = true, int priority = 100)
                => UnityAddressables.LoadSceneAsync(key, loadSceneParameters, activateOnLoad, priority);
            public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneParameters loadSceneParameters, SceneReleaseMode releaseMode, bool activateOnLoad = true, int priority = 100)
                => UnityAddressables.LoadSceneAsync(key, loadSceneParameters, releaseMode, activateOnLoad, priority);

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
                        if (gameObject == null)
                        {
                            Debug.LogWarning($"{nameof(Addressables)}: GameObject '{key}' not found.");
                            return UnityAddressables.ResourceManager.CreateCompletedOperation<T>(null, null);
                        }
                        if (gameObject.TryGetComponent(out T component) == false)
                        {
                            Debug.LogWarning($"{nameof(Addressables)}: Component '{typeof(T).Name}' not found in key: {key}. GameObject: {gameObject.name}");
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
                // Reuse the async single-loader so the key existence check stays asynchronous.
                // (KeyExists relies on WaitForCompletion, which is not supported on WebGL.)
                var handles = new List<AsyncOperationHandle<T>>();
                foreach (var key in keys) handles.Add(LoadPrefabAsync<T>(key));

                return CreateGroupOperation(handles);
            }

            private static AsyncOperationHandle<IList<T>> LoadManyObjectOrderedAsync<T>(IList<string> keys) where T : Object
            {
                // Reuse the async single-loader so the key existence check stays asynchronous.
                // (KeyExists relies on WaitForCompletion, which is not supported on WebGL.)
                var handles = new List<AsyncOperationHandle<T>>();
                foreach (var key in keys) handles.Add(LoadObjectAsync<T>(key));

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
