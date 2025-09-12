using HisaCat.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HisaCat.HUE.UI.Windows
{
    public class WindowSystem : WindowSystemBase
    {
        public static readonly string DefaultWindowPrefabFolder = "<Your default window prefab's directory>";
        public override string GetDefaultWindowPath(Type type) => UniPath.Join(DefaultWindowPrefabFolder, type.Name, $"{type.Name}.prefab");

        public override WindowBase LoadWindowBasePrefabFrom(string path)
        {
            // ==============================
            // Load your window prefab here.
            // ==============================
            return null;
        }

        public override IEnumerator LoadOrderedWindowBasePrefabsFrom(List<string> paths, Action<IList<WindowBase>> onCompleted)
        {
            // ==============================
            // Load your window prefabs here.
            // ==============================
            // Example:
            // var op = HisaCat.HUE.Assets.AssetLoader.Addressables.LoadManyOrderedAsync<WindowBase>(paths);
            // yield return op;
            // onCompleted.Invoke(op.Result);

            yield break;
        }

        protected override void Init()
        {
            // ==============================
            // Delegate ui events here.
            // ==============================
            // Example:
            // Inputs.InputManager.Maps.UI.OnNavigateStarted += OnUINavigateStarted;
            // Inputs.InputManager.Maps.UI.OnCancelStarted += OnCancelStarted;
        }

        protected override void Dispose()
        {
            // ==============================
            // Undelegate ui events here.
            // ==============================
            // Example:
            // Inputs.InputManager.Maps.UI.OnNavigateStarted -= OnUINavigateStarted;
            // Inputs.InputManager.Maps.UI.OnCancelStarted -= OnCancelStarted;
        }

        // ==============================
        // Define ui event callbacks here.
        // ==============================
        // Example:
        // private void OnUINavigateStarted(InputAction.CallbackContext ctx) => base.OnUINavigateStarted();
        // private void OnCancelStarted(InputAction.CallbackContext ctx) => base.OnBackButton();
    }
}
