using HisaCat.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace HisaCat.HUE.Localization
{
    public static class LocalizationManager
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                _selectedLanguage = LocalizationSettings.DefaultLanguage;
                OnLanguageChanged = null;
                localizedTextsByPath = null;
            }
        }
#pragma warning restore IDE0051
#endif

        #region Language selection
        private static SystemLanguage _selectedLanguage = LocalizationSettings.DefaultLanguage;
        public static SystemLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (LocalizationSettings.SupportLanguages.Contains(value) == false)
                {
                    Debug.LogError($"[{nameof(LocalizationManager)}] language \"{value}\" does not support! (Not exists in \"{nameof(LocalizationSettings.SupportLanguages)}\")");
                    return;
                }

                if (_selectedLanguage == value)
                    return;

                var prevLang = _selectedLanguage;
                _selectedLanguage = value;

                OnLanguageChangedCallback(prevLang, _selectedLanguage);
            }
        }
        public delegate void OnLanguageChangedEventHandler(SystemLanguage prevLang, SystemLanguage curLang);
        public static event OnLanguageChangedEventHandler OnLanguageChanged = null;
        private static void OnLanguageChangedCallback(SystemLanguage prevLang, SystemLanguage curLang)
        {
            OnLanguageChanged?.Invoke(prevLang, curLang);
        }
        #endregion

        private static Dictionary<string, LocalizedTexts> localizedTextsByPath = null;
        public static void ClearLoadedLocalizedTexts() => localizedTextsByPath = null;

        public static bool Exists(string key) => Exists(SelectedLanguage, key);
        public static bool Exists(SystemLanguage lang, string key) => Exists(lang, LocalizationSettings.LocalizedJsonsPath, key);
        public static bool Exists(string localizedJsonsFolderPath, string key) => Exists(SelectedLanguage, localizedJsonsFolderPath, key);
        public static bool Exists(SystemLanguage lang, string localizedJsonsFolderPath, string key)
        {
            localizedTextsByPath ??= new Dictionary<string, LocalizedTexts>(StringComparer.OrdinalIgnoreCase);

            if (localizedTextsByPath.ContainsKey(localizedJsonsFolderPath) == false)
                localizedTextsByPath.Add(localizedJsonsFolderPath, new LocalizedTexts(localizedJsonsFolderPath, LocalizationSettings.LoadJsonHandler));

            return localizedTextsByPath[localizedJsonsFolderPath].Exists(lang, key);
        }

        public static string Load(string key) => Load(LocalizationManager.SelectedLanguage, key);
        public static string Load(SystemLanguage lang, string key) => Load(lang, LocalizationSettings.LocalizedJsonsPath, key);
        public static string Load(string localizedJsonsFolderPath, string key) => Load(LocalizationManager.SelectedLanguage, localizedJsonsFolderPath, key);
        public static string Load(SystemLanguage lang, string localizedJsonsFolderPath, string key, SystemLanguage? fallback = null)
        {
            if (localizedTextsByPath == null)
                localizedTextsByPath = new Dictionary<string, LocalizedTexts>(StringComparer.OrdinalIgnoreCase);

            if (localizedTextsByPath.ContainsKey(localizedJsonsFolderPath) == false)
                localizedTextsByPath.Add(localizedJsonsFolderPath, new LocalizedTexts(localizedJsonsFolderPath, LocalizationSettings.LoadJsonHandler));

            return localizedTextsByPath[localizedJsonsFolderPath].Load(lang, key, fallback == null ? LocalizationSettings.FallbackLanguage : fallback.Value);
        }
    }

    public class LocalizedTexts
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("HisaCat/Localization/Create Localized Text Templates")]
        public static void CreateTemplate()
        {
            var path = UnityEditor.EditorUtility.SaveFolderPanel("Path", "Assets", "Localized");
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

            UnityEditor.EditorUtility.DisplayDialog("Localization", "Localized String Templates created and saved.", "Ok");
            UnityEditor.AssetDatabase.Refresh();
            var folderAssetPath = path;
            if (folderAssetPath.StartsWith(Application.dataPath))
                folderAssetPath = "Assets" + folderAssetPath.Substring(Application.dataPath.Length);
            var folderAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderAssetPath);
            UnityEditor.EditorGUIUtility.PingObject(folderAsset);
        }
#endif

        public delegate string LoadJsonEventHandler(string localizedJsonsFolderPath, SystemLanguage lang);

        private readonly Dictionary<SystemLanguage, Dictionary<string, string>> localizedTextsByLanguage = null;
        private readonly LoadJsonEventHandler loadJsonHandler = null;
        public readonly string LocalizedJsonsFolderPath = string.Empty;
        public LocalizedTexts(string localizedJsonsFolderPath, LoadJsonEventHandler loadJsonHandler = null)
        {
            this.LocalizedJsonsFolderPath = localizedJsonsFolderPath;
            this.localizedTextsByLanguage = new Dictionary<SystemLanguage, Dictionary<string, string>>();
            this.loadJsonHandler = loadJsonHandler ?? DefaultLoadJsonEventHandler;
        }

        public string DefaultLoadJsonEventHandler(string localizedJsonsFolderPath, SystemLanguage lang)
        {
            var jsonPath = string.IsNullOrEmpty(localizedJsonsFolderPath) ? lang.ToLocaleString() : UniPath.Combine(localizedJsonsFolderPath, lang.ToLocaleString());
            var jsonAsset = Resources.Load<TextAsset>(jsonPath);
            return jsonAsset == null ? null : jsonAsset.text;
        }

        private bool IsLanguageLoaded(SystemLanguage lang) => this.localizedTextsByLanguage.ContainsKey(lang);
        private void LoadLanguage(SystemLanguage lang)
        {
            if (IsLanguageLoaded(lang)) return;

            var json = this.loadJsonHandler(this.LocalizedJsonsFolderPath, lang);

            if (string.IsNullOrEmpty(json))
            {
                if (LocalizationSettings.PrintMissingLanguageLogs)
                    Debug.LogWarning($"[{nameof(LocalizationManager)}] {nameof(Load)}: Cannot find {lang} language.");
            }

            // Parse json.
            var keyTextPair = json == null ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            this.localizedTextsByLanguage.Add(lang, keyTextPair);
        }

        public bool Exists(SystemLanguage lang, string key)
        {
            if (key == null) return false;

            // Load if target language not exists in dictionary.
            if (IsLanguageLoaded(lang) == false) LoadLanguage(lang);

            return this.localizedTextsByLanguage[lang].ContainsKey(key);
        }

        public string Load(SystemLanguage lang, string key, SystemLanguage fallbackLang)
        {
            if (key == null) return null;

            var keyExists = Exists(lang, key);

            // Exists(lang, key) already loads language if not exists so we don't need to load it again.
            // if (IsLanguageLoaded(lang) == false) LoadLanguage(lang);

            // Fallback if key not exists in target language.
            if (keyExists == false)
            {
                if (LocalizationSettings.PrintMissingKeyLogs)
                    Debug.LogWarning($"[{nameof(LocalizationManager)}] {nameof(Load)}: Cannot find key \"{key}\" at {lang} language.");

                if (lang == fallbackLang) return key;

                return Load(fallbackLang, key, fallbackLang);
            }

            var value = this.localizedTextsByLanguage[lang][key];

            // 기본적으로 한국어 조사 치환은 "{item}%이가% 필요합니다." 등의 문자열에 대해
            // NamedFormat 이후의 결과에 대해서 사용해야 함으로,
            // 여기서 전역으로 처리할 필요는 없습니다.
            // If language is Korean, resolve josa tokens
            // if (lang == SystemLanguage.Korean)
            //     value = KoreanUtility.JosaHelper.ResolveJosaTokens(value);

            return value;
        }
    }
}
