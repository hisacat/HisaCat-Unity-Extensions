using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using Newtonsoft.Json;

namespace HisaCat.HUE.Localization
{
    [InitializeOnLoad]
    public static class LocalizationEditorUtility
    {
        static LocalizationEditorUtility()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    // Reset selected language to default language when exiting play mode.
                    LocalizationManager.SelectedLanguage = LocalizationSettings.DefaultLanguage;

                    // LocalizationManager clears OnLanguageChanged when entering play mode (domain reload disabled).
                    // Re-subscribe when returning to edit mode so language changes still update fonts.
                    SubscribeToLanguageChanged();
                }
            }

            SubscribeToLanguageChanged();
        }

        /// <summary>
        /// Subscribe to <see cref="LocalizationManager.OnLanguageChanged"/> event.
        /// </summary>
        private static void SubscribeToLanguageChanged()
        {
            // Unsubscribe first so this is idempotent (safe to call multiple times).
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private static void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            UpdateIEditorLocalizedTexts();
        }

        public static void UpdateIEditorLocalizedTexts()
        {
            if (Application.isPlaying) return;
            if (LocalizationSettings.AutoUpdateLocalizedTextOnEditor == false) return;

            // Force update localized texts.
            var texts = FindInterfacesOfType<IEditorLocalizedTextUpdateable>();
            static IEnumerable<T> FindInterfacesOfType<T>(bool includeInactive = false)
            {
                var scenes = new List<Scene>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    scenes.Add(SceneManager.GetSceneAt(i));

                // Scene texts.
                var texts = scenes.SelectMany(e =>
                    e.GetRootGameObjects().SelectMany(go =>
                        go.GetComponentsInChildren<T>(includeInactive)));

                // Prefab stage texts.
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                    texts = texts.Concat(prefabStage.scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive)));

                return texts;
            }

            foreach (var text in texts)
                text.UpdateTextOnEditor();
        }

        [MenuItem("HisaCat/Localization/Create Localized Texts Templates")]
        public static void CreateTemplate()
        {
            var path = EditorUtility.SaveFolderPanel("Path", "Assets", "Localized");
            if (string.IsNullOrEmpty(path))
                return;

            var data = new Dictionary<string, string>();
            for (int i = 0; i < 3; i++)
                data.Add($"key{i}", "text");

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var langs = new SystemLanguage[] { SystemLanguage.English, SystemLanguage.Korean, SystemLanguage.Japanese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional };
            foreach (var lang in langs)
            {
                var filePath = System.IO.Path.Combine(path, $"{lang.ToLocaleString()}.json");
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"[{nameof(LocalizationManager)}] Template for ${lang} created at ${filePath}");
            }

            EditorUtility.DisplayDialog("Localization", "Localized String Templates created and saved.", "Ok");
            AssetDatabase.Refresh();
            var folderAssetPath = path;
            if (folderAssetPath.StartsWith(Application.dataPath))
                folderAssetPath = "Assets" + folderAssetPath.Substring(Application.dataPath.Length);
            var folderAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderAssetPath);
            EditorGUIUtility.PingObject(folderAsset);
        }
    }
}
