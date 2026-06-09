
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
                        // TODO: 설정 어셋이 존재하는데도 불구하고
                        // 프로젝트를 최초 열 때 (cold start / git clean -dfx 이후 등) 에는 Resources.Load에 의해 설정 어셋이 불러와지지 않는다.
                        // 'InitializeOnLoad' 나 static field를 통해서 Resources의 indexing이 완료되기 전에 LoadSettingsAsset():Resources.Load가 호출되어
                        // 생기는 이슈인 것으로 판단된다.
                        // 이는 cold start 시에만 발생하는 warning임으로, 첫 프로젝트 구동 이후와 빌드와는 관련이 없기에 우선순위는 낮은 이슈이다.
                        // 또한 이를 수정하기 위해서는 차라리 전체적으로 이 Localization 관련 코드를 리펙토링하는 것이 더 나을 수 있다.
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
