using HisaCat.Localization;
using HisaCat.UnityExtensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HisaCat.Fonts
{
    [CreateAssetMenu(fileName = "I18NFontAsset", menuName = "HisaCat/I18NFontDataAsset", order = 2)]
    public class I18NFontDataAsset : ScriptableObject
    {
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

#if UNITY_EDITOR
        static I18NFontDataAsset()
        {
            // TODO: Sometime this constructor is not called. need to fix it.

            // In the editor environment, when the language changes while not in play mode,
            // updates the fonts of all I18NFontDataAssets.
            // This allows you to preview multilingual font application results in real time during development.
            // * This is an editor-only logic and is not included in the build.
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            static void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
            {
                if (Application.isPlaying == false)
                {
                    var assetGUIDs = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(I18NFontDataAsset)}");
                    foreach (var assetGUID in assetGUIDs)
                    {
                        var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
                        var i18nFontDataAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<I18NFontDataAsset>(assetPath);
                        if (i18nFontDataAsset == null) continue;
                        i18nFontDataAsset.UpdateBaseFontFallbacks(curLang);
                    }
                }
            }
        }
#endif

        public TMP_FontAsset BaseFont => this.m_BaseFont;
        [Header("The Base font. Use en_US for default.")]
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

        public void UpdateBaseFontFallbacks(SystemLanguage language)
        {
            // Clear fallback fonts.
            this.BaseFont.fallbackFontAssetTable.Clear();

            // Add target language font.
            I18NFont targetFont = this.I18NFonts.FirstOrDefaultNonAlloc(Predicate);
            bool Predicate(I18NFont e) => e.language == language;

            if (targetFont != null)
            {
                if (targetFont.Font == null)
                    throw new System.ArgumentNullException($"I18NFont \"{this.name}\" has null font for language \"{language}\"!");

                this.BaseFont.fallbackFontAssetTable.Add(targetFont.Font);
            }

            // Add other language fonts.
            int count = this.I18NFonts.Count;
            for (int i = 0; i < count; i++)
            {
                var i18nFont = this.I18NFonts[i];
                if (this.BaseFont.fallbackFontAssetTable.Contains(i18nFont.Font) == false)
                    this.BaseFont.fallbackFontAssetTable.Add(i18nFont.Font);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this.BaseFont);
            // UnityEditor.AssetDatabase.SaveAssetIfDirty(this.BaseFont);
#endif

            ForceApplyTMPFallbackFontsGlobally();
        }
        private void ForceApplyTMPFallbackFontsGlobally()
        {
            // Clear fallback font glyph cache.
            TMP_ResourceManager.ClearFontAssetGlyphCache();

            // Force update TextMeshPro components mesh. (Without this, font will does not updated in Build environment)
            var tmpTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var tmpTextsCount = tmpTexts.Length;
            for (int i = 0; i < tmpTextsCount; i++)
            {
                var tmpText = tmpTexts[i];
                tmpText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            }
        }
    }
}
