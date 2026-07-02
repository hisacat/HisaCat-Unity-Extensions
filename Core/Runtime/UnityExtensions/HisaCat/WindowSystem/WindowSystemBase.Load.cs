using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.UI.Windows
{
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
        #region Abstract Methods
        /// <summary>
        /// Get the default window path.
        /// </summary>
        /// <param name="type">The type of the window.</param>
        /// <returns></returns>
        public abstract string GetDefaultWindowPath(System.Type type);

        /// <summary>
        /// Load the window base prefab from the path.
        /// </summary>
        /// <param name="path">The path of the window base prefab.</param>
        /// <returns></returns>
        public abstract WindowBase LoadWindowBasePrefabFromSource(string path);

        /// <summary>
        /// Load the window base prefab from the path asynchronously.
        /// </summary>
        /// <param name="path">The path of the window base prefab.</param>
        /// <param name="onCompleted">The callback when the window base prefab is loaded.</param>
        /// <returns>The enumerator.</returns>
        protected abstract IEnumerator LoadWindowBasePrefabFromSource(string path, System.Action<WindowBase> onCompleted);
        /// <summary>
        /// Load the window base prefabs from the paths.<br/>
        /// The order of the windows must be the same as the order of the paths.
        /// </summary>
        /// <param name="paths">The paths of the window base prefabs.</param>
        /// <param name="onCompleted">The callback when the windows are loaded.</param>
        /// <returns></returns>
        protected abstract IEnumerator LoadOrderedWindowBasePrefabsFromSource(List<string> paths, System.Action<IList<WindowBase>> onCompleted);
        #endregion Abstract Methods

        public static string GetDefaultWindowPath<T>() where T : WindowBase
        {
            if (InstanceExist == false)
                throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(GetDefaultWindowPath)}: Instance is not initialized yet.");

            return Instance.GetDefaultWindowPath(typeof(T));

        }
        private static string GetDefaultWindowPath_Internal(System.Type type)
        {
            if (InstanceExist == false)
                throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(GetDefaultWindowPath_Internal)}: Instance is not initialized yet.");

            return Instance.GetDefaultWindowPath(type);
        }

        public static IEnumerator PreloadWindow<T>() where T : WindowBase
        {
            var windowPath = Instance.GetDefaultWindowPath(typeof(T));

            WindowBase window = null;
            {
                yield return Instance.LoadWindowBasePrefabFromSource(windowPath, OnLoaded);
                void OnLoaded(WindowBase _window) => window = _window;
            }
            if (Instance.loadedWindowPrefabsCache.ContainsKey(windowPath))
            {
                Debug.LogError($"[{nameof(WindowSystemBase)}] {nameof(PreloadWindow)}: Window '{window.name}' path '{windowPath}' is already preloaded.");
                yield break;
            }
            Instance.loadedWindowPrefabsCache.Add(windowPath, window);
            Debug.Log($"[{nameof(WindowSystemBase)}] Window '{window.name}' from path '{windowPath}' preloaded. (type: {typeof(T).Name})");

            yield break;
        }
        public static IEnumerator PreloadAllWindowTypePrefabsRoutine()
        {
            // Enumerate the dictionary once so that the type/path order stays consistent
            // with the loaded windows returned below (which preserve the requested order).
            var windowTypes = new List<System.Type>(WindowTypesDic.Values);
            var windowPaths = new List<string>(windowTypes.Count);
            foreach (var windowType in windowTypes)
                windowPaths.Add(Instance.GetDefaultWindowPath(windowType));

            IList<WindowBase> windows = null;
            {
                yield return Instance.LoadOrderedWindowBasePrefabsFromSource(windowPaths, OnLoaded);
                void OnLoaded(IList<WindowBase> _windows) => windows = _windows;
            }

            int loadedCount = 0;
            for (int i = 0; i < windowPaths.Count; i++)
            {
                (var windowType, var windowPath) = (windowTypes[i], windowPaths[i]);

                var window = (windows != null && i < windows.Count) ? windows[i] : null;
                if (window == null) continue;

                // Validate the loaded type just like the synchronous LoadWindowPrefab path does,
                // so the cache content is identical regardless of how it was populated.
                var expectedType = windowType;
                var loadedWindowType = window.GetType();
                if (loadedWindowType != expectedType)
                {
                    Debug.LogError(
                        $"[{nameof(WindowSystemBase)}] {nameof(PreloadAllWindowTypePrefabsRoutine)}: Window '{window.name}' type '{loadedWindowType.Name}' is not '{expectedType.Name}'."
                        + $"\r\nPath: '{windowPath}'");
                    continue;
                }

                // Cache the preloaded prefab so later (synchronous) LoadWindowPrefab calls resolve
                // from cache instead of triggering a load. This is mandatory on WebGL, where
                // synchronous Addressable loading (WaitForCompletion) is not supported at all.
                // Guard against duplicate/re-preload before adding, so a re-preload is reported
                // instead of silently overwriting the cached prefab.
                if (Instance.loadedWindowPrefabsCache.ContainsKey(windowPath))
                {
                    Debug.LogError($"[{nameof(WindowSystemBase)}] {nameof(PreloadAllWindowTypePrefabsRoutine)}: Window '{window.name}' path '{windowPath}' is already preloaded.");
                    continue;
                }
                Instance.loadedWindowPrefabsCache.Add(windowPath, window);
                loadedCount++;
            }
            Debug.Log($"[{nameof(WindowSystemBase)}] {loadedCount} of {WindowTypesDic.Count} window prefabs preloaded.");

            yield break;
        }

        public static T LoadWindowPrefab<T>(string specialPath = null) where T : WindowBase
            => LoadWindowPrefab(typeof(T), specialPath) as T;

        /// <summary>
        /// Load the window prefab from the path.
        /// </summary>
        /// <param name="type">The type of the window.</param>
        /// <param name="specialPath">The special path of the window.</param>
        /// <returns>The window prefab.</returns>
        public static WindowBase LoadWindowPrefab(System.Type type, string specialPath = null)
        {
            if (InstanceExist == false)
                throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(LoadWindowPrefab)}: Instance is not initialized yet.");

            var windowPath = string.IsNullOrEmpty(specialPath) ?
                GetDefaultWindowPath_Internal(type) : $"Windows/{specialPath}";

            // Return window prefab if it already cached.
            if (Instance.loadedWindowPrefabsCache.TryGetValue(windowPath, out var window))
                return window;

            // It not, load window prefab from the path.
            window = Instance.LoadWindowBasePrefabFromSource(windowPath);
            if (window != null)
            {
                var loadedWindowType = window.GetType();
                if (loadedWindowType != type)
                {
                    Debug.LogError(
                        $"[{nameof(WindowSystemBase)}] {nameof(LoadWindowPrefab)}: Window '{window.name}' type '{loadedWindowType.Name}' is not '{type.Name}'."
                        + $"\r\nPath: '{windowPath}'");
                    return null;
                }

                Instance.loadedWindowPrefabsCache.Add(windowPath, window);
            }
            return window;
        }
        public static WindowBase ShowNewInstance(System.Type type, string specialPath = null, bool immediately = false)
        {
            if (InstanceExist == false)
                throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(ShowNewInstance)}: Instance is not initialized yet.");

            WindowBase windowResource = LoadWindowPrefab(type, specialPath);
            if (windowResource == null)
            {
                Debug.LogError(
                    $"[{nameof(WindowSystemBase)}] {nameof(ShowNewInstance)}: Cannot find window '{type.Name}'." +
                    $"\r\nPath: '{(specialPath == null ? GetDefaultWindowPath_Internal(type) : specialPath)}'");
                return null;
            }

            var window = Instantiate(windowResource, Instance.WindowArea.transform);
            window.name = windowResource.name;
            window.DestroyOnClosed = true;

            if (Show(window, immediately))
                return window;
            else
                return null;
        }
        public static T ShowNewInstance<T>(string specialPath = null, bool immediately = false) where T : WindowBase
            => ShowNewInstance(typeof(T), specialPath, immediately) as T;
    }
}
