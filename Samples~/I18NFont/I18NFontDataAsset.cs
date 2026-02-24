using HisaCat.Localization;
using HisaCat.UnityExtensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    [CreateAssetMenu(fileName = "I18NFontDataAsset", menuName = "HisaCat/HUE/I18N Fonts/I18N Font Data Asset", order = 2)]
    [DisallowMultipleComponent]
    public class I18NFontDataAsset : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            var assetGUIDs = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(I18NFontDataAsset)}");
            foreach (var assetGUID in assetGUIDs)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
                var i18nFontDataAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<I18NFontDataAsset>(assetPath);
                if (i18nFontDataAsset == null) continue;

                // This is ScriptableObject so isInitialized field is doe not reset to default value even it is not static.
                // So we need to reset it manually on Editor Environment.
                i18nFontDataAsset.isInitialized = false;
            }
        }
#pragma warning restore IDE0051
#endif

        private bool isInitialized = false;
        public void Initialize()
        {
            if (this.isInitialized == true)
            {
                Debug.LogWarning($"[{nameof(I18NFontDataAsset)}] \"{this.name}\" ({this.GetInstanceID()}) is already initialized.");
                return;
            }

            LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            UpdateBaseFontFallbacks(LocalizationManager.SelectedLanguage);
            this.isInitialized = true;
        }
        private void OnDestroy()
        {
            if (this.isInitialized == true)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }
        void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            UpdateBaseFontFallbacks(curLang);
        }

        public void UpdateBaseFontFallbacks(SystemLanguage language)
            => I18NFontUtility.UpdateBaseFontFallbacks(this, this.BaseFont, language, forceRefreshAllTmpFonts: true);

        public TMP_FontAsset BaseFont => this.m_BaseFont;
        [SerializeField] private TMP_FontAsset m_BaseFont = null;
        public IReadOnlyList<I18NFont> I18NFonts => this.m_I18NFonts;
        [SerializeField] private List<I18NFont> m_I18NFonts = null;

        [System.Serializable]
        public class I18NFont
        {
            public SystemLanguage language => this.m_Language;
            [SerializeField] private SystemLanguage m_Language = SystemLanguage.Unknown;
            public TMP_FontAsset Font => this.m_Font;
            [SerializeField] private TMP_FontAsset m_Font = null;
        }
    }
}
