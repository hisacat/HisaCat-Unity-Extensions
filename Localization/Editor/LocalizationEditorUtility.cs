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

            var langs = new SystemLanguage[] { SystemLanguage.English, SystemLanguage.Korean, SystemLanguage.Japanese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional };
            foreach (var lang in langs)
            {
                var data = new Dictionary<string, string>
                {
                    { "title", Application.productName },
                    { "company", Application.companyName },
                    { "version_format", "v{version} {version_alias}" }
                };
                void AddAdditionalTexts(string machine_translation_info, string button_ok, string button_yes, string button_no, string button_confirm, string button_cancel)
                {
                    data.Add("machine_translation_info", machine_translation_info);
                    data.Add("button.ok", button_ok);
                    data.Add("button.yes", button_yes);
                    data.Add("button.no", button_no);
                    data.Add("button.confirm", button_confirm);
                    data.Add("button.cancel", button_cancel);
                    data.Add("empty_string", "");
                }
                switch (lang)
                {
                    case SystemLanguage.English:
                        AddAdditionalTexts(
                            "This game uses machine translation for some parts."
                            + "\r\nIf you find any mistranslations, please report them through the community to help improve the game.",
                            "Ok", "Yes", "No", "Confirm", "Cancel");
                        break;
                    case SystemLanguage.Korean:
                        AddAdditionalTexts(
                            "본 게임은 일부 기계 번역을 사용하고 있습니다."
                            + "\r\n오역이 있을 시 커뮤니티를 통해 제보해주시면 게임 발전에 도움이 됩니다.",
                            "확인", "예", "아니요", "확인", "취소");
                        break;
                    case SystemLanguage.Japanese:
                        AddAdditionalTexts(
                            "本ゲームでは一部に機械翻訳を使用しています。"
                            + "\r\n誤訳がある場合は、コミュニティを通じてご報告いただけますと、ゲームの改善に役立ちます。",
                            "OK", "はい", "いいえ", "確認", "キャンセル");
                        break;
                    case SystemLanguage.ChineseSimplified:
                        AddAdditionalTexts(
                            "本游戏部分内容使用了机器翻译。"
                            + "\r\n如有误译，欢迎通过社区反馈，这将有助于改进游戏。",
                            "确定", "是", "否", "确认", "取消");
                        break;
                    case SystemLanguage.ChineseTraditional:
                        AddAdditionalTexts(
                            "本遊戲部分內容使用了機器翻譯。"
                            + "\r\n如有誤譯，歡迎透過社群回報，這將有助於改進遊戲。",
                            "確定", "是", "否", "確認", "取消");
                        break;
                }
                var json = JsonConvert.SerializeObject(data, Formatting.Indented) + "\n";

                var filePath = System.IO.Path.Combine(path, $"{lang.ToLocaleString()}.json");
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"[{nameof(LocalizationManager)}] Template for ${lang} created at ${filePath}");
            }

            EditorUtility.DisplayDialog("Localization", "Localized String Templates created and saved.", "Ok");
            AssetDatabase.Refresh();
            var folderAssetPath = path;
            if (folderAssetPath.StartsWith(Application.dataPath))
                folderAssetPath = "Assets" + folderAssetPath.Substring(Application.dataPath.Length);
            var folderAsset = AssetDatabase.LoadAssetAtPath<Object>(folderAssetPath);
            EditorGUIUtility.PingObject(folderAsset);
        }
    }
}
