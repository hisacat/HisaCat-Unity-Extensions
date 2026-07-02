using HisaCat.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HisaCat.HUE.UI.Windows
{
    public class WindowSystem : WindowSystemBase
    {
        public static readonly string DefaultWindowPrefabFolder = ""; // <Your default window prefab's directory>
        public override string GetDefaultWindowPath(System.Type type) => UniPath.Join(DefaultWindowPrefabFolder, type.Name, $"{type.Name}.prefab");

        public sealed override WindowBase LoadWindowBasePrefabFromSource(string path)
        {
            // ==============================
            // Load your window prefab here.
            // ==============================
#if UNITY_WEBGL
            // Synchronous Addressable loading (WaitForCompletion) is not supported on WebGL.
            // Prefabs must be preloaded via PreloadWindow / PreloadAllWindowPrefabsRoutine beforehand.
            UnityEngine.Debug.LogError(
                $"[{nameof(WindowSystem)}] Synchronous window load is not supported on WebGL: {path}."
                + $"Call {nameof(PreloadWindow)} or {nameof(PreloadAllWindowPrefabsRoutine)} beforehand.");
            return null;
#else
            return Assets.AssetLoader.Addressables.LoadSync<WindowBase>(path);
#endif
        }

        protected sealed override IEnumerator LoadWindowBasePrefabFromSource(string path, Action<WindowBase> onCompleted)
        {
            // ==============================
            // Load your window prefabs here.
            // ==============================
            var op = Assets.AssetLoader.Addressables.LoadAsync<WindowBase>(path);
            yield return op;
            onCompleted.Invoke(op.Result);

            yield break;
        }

        protected sealed override IEnumerator LoadOrderedWindowBasePrefabsFromSource(List<string> paths, Action<IList<WindowBase>> onCompleted)
        {
            // ==============================
            // Load your window prefabs here.
            // ==============================
            var op = Assets.AssetLoader.Addressables.LoadManyOrderedAsync<WindowBase>(paths);
            yield return op;
            onCompleted.Invoke(op.Result);

            yield break;
        }

        protected override void Init()
        {
            // ==============================
            // Delegate ui events here.
            // ==============================
            Inputs.InputManager.Maps.UI.OnNavigatePerformed += OnUINavigatePerformed;
            Inputs.InputManager.Maps.UI.OnCancelPerformed += OnCancelPerformed;
        }

        protected override void Dispose()
        {
            // ==============================
            // Undelegate ui events here.
            // ==============================
            Inputs.InputManager.Maps.UI.OnNavigatePerformed -= OnUINavigatePerformed;
            Inputs.InputManager.Maps.UI.OnCancelPerformed -= OnCancelPerformed;
        }

        // ==============================
        // Define ui event callbacks here.
        // ==============================
        private void OnUINavigatePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => base.OnUINavigatePerformed();
        private void OnCancelPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => base.OnBackButton();
    }
}
