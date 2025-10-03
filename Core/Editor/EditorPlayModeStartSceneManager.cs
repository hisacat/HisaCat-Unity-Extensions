using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using HisaCat.HUE.Settings;

namespace HisaCat.HUE
{
    [InitializeOnLoad]
    public static class EditorPlayModeStartSceneManager
    {
        private static bool IsInitializedOnce = false;
        static EditorPlayModeStartSceneManager()
        {
            // Hotfix: Some Unity Editor versions have an issue where
            // EditorBuildSettings.scenes is empty during InitializeOnLoad initialization.
            // Using delayCall to defer execution ensures scenes are properly loaded before accessing them.
            // * This issue is confirmed in Unity 6000.2.6f2.
            EditorApplication.delayCall -= Callback;
            EditorApplication.delayCall += Callback;
            static void Callback()
            {
                if (IsInitializedOnce) return;
                UpdateStartScene();
                IsInitializedOnce = true;
            }
        }

        public static void SetStartScene(string scenePath)
        {
            var settings = HueSettings.Instance.GizmosSettings.EditorPlayModeStartSceneSettings;
            settings.UseFirstBuildScene = false;
            settings.StartScenePath = scenePath;

            UpdateStartScene();
        }

        public static void SetStartSceneAsFirstBuildScene()
        {
            var settings = HueSettings.Instance.GizmosSettings.EditorPlayModeStartSceneSettings;
            settings.UseFirstBuildScene = true;
            settings.StartScenePath = string.Empty;

            UpdateStartScene();
        }

        internal static void UpdateStartScene()
        {
            var settings = HueSettings.Instance.GizmosSettings.EditorPlayModeStartSceneSettings;
            if (settings.Enable == false) { ResetStartScene(); return; }

            SceneAsset sceneAsset = null;
            if (settings.UseFirstBuildScene)
            {
                var buildScenes = EditorBuildSettings.scenes;
                if (buildScenes == null || buildScenes.Length == 0) { ResetStartScene(); return; }

                var pathOfFirstScene = buildScenes[0].path;
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
            }
            else if (string.IsNullOrEmpty(settings.StartScenePath) == false)
            {
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.StartScenePath);
            }

            if (sceneAsset == null) { ResetStartScene(); return; }

            EditorSceneManager.playModeStartScene = sceneAsset;
            Debug.Log($"<b>[{nameof(EditorPlayModeStartSceneManager)}] Start Scene is set to '{sceneAsset.name}'</b>", sceneAsset);

            static void ResetStartScene()
            {
                EditorSceneManager.playModeStartScene = null;
                Debug.Log($"<b>[{nameof(EditorPlayModeStartSceneManager)}] Start Scene is reset</b>");
            }
        }
    }
}
