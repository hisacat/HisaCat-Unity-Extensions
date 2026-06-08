
using HisaCat.HUE.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Localization
{
    public static class LocalizationSettings
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                _settings = null;
            }
        }
#pragma warning restore IDE0051
#endif

        public const string SettingsAssetPath = LocalizationSettingsAsset.DefaultAssetPath;
        private static LocalizationSettingsAsset _settings = null;
        private static LocalizationSettingsAsset Settings
        {
            get
            {
                if (_settings == null)
                {
                    LoadSettingsAsset();
                    if (_settings == null)
                    {
                        _settings = LocalizationSettingsAsset.CreateDefaultInstance();
                        // TODO: 리소스가 존재하는 상태로 프로젝트를 최초 열 때 경고가 뜨는 것 수정.
                        // 아마 처음 열 때에는 해당 리소스가 LoadSettingsAsset():Resources.Load 에 의해 불러와지지 않는 것으로 추정됨.
                        Debug.LogWarning($"[{nameof(LocalizationSettings)}] No settings asset found at \"Resources/{SettingsAssetPath}\". Use default instance.");
                    }
                }
                return _settings;
            }
        }
        public static void ReloadSettingsAsset()
        {
            _settings = null;
            LoadSettingsAsset();
        }
        public static void LoadSettingsAsset()
        {
            if (ConditionLog.Log(_settings != null, $"[{nameof(LocalizationSettings)}] Settings asset already loaded.")) return;
            _settings = Resources.Load<LocalizationSettingsAsset>(SettingsAssetPath);
        }

        public const string LocalizedJsonsPath = "Localization";
        public static SystemLanguage DefaultLanguage => Settings.DefaultLanguage;
        public static SystemLanguage FallbackLanguage => Settings.FallbackLanguage;
        public static HashSet<SystemLanguage> SupportLanguages => Settings.SupportLanguages;
        public static bool PrintMissingLanguageLogs => Settings.PrintMissingLanguageLogs;
        public static bool PrintMissingKeyLogs => Settings.PrintMissingKeyLogs;

        public static bool AutoUpdateLocalizedTextOnEditor => Settings.AutoUpdateLocalizedTextOnEditor;

        public static LocalizedTexts.LoadJsonEventHandler LoadJsonHandler { get; private set; }
        public static void SetLoadJsonHandler(LocalizedTexts.LoadJsonEventHandler loadJsonHandler)
        {
            LoadJsonHandler = loadJsonHandler;

            //Example of LoadJsonHandler
            //public LocalizedTexts.LoadJsonEventHandler LoadJsonHandler = (path, lang)=>
            //{
            //    var jsonPath = string.IsNullOrEmpty(path) ? LocalizedTexts.GetLocaleStr(lang) : $"{path}/{LocalizedTexts.GetLocaleStr(lang)}";
            //    var jsonAsset = Resources.Load<TextAsset>(jsonPath);
            //    return jsonAsset == null ? null : jsonAsset.text;
            //};
        }
    }
}
