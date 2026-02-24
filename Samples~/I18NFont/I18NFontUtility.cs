using UnityEngine;
using TMPro;
using HisaCat.UnityExtensions;

namespace HisaCat.HUE.Fonts
{
    public static class I18NFontUtility
    {
        public static void UpdateBaseFontFallbacks(I18NFontDataAsset i18nFontData, TMP_FontAsset baseFont, SystemLanguage language, bool forceRefreshAllTmpFonts)
        {
            // Clear fallback fonts.
            baseFont.fallbackFontAssetTable.Clear();

            // Add target language font.
            var targetFont = i18nFontData.I18NFonts.FirstOrDefaultNonAlloc(Predicate);
            bool Predicate(I18NFontDataAsset.I18NFont e) => e.language == language;

            if (targetFont != null)
            {
                if (targetFont.Font == null)
                    throw new System.ArgumentNullException($"I18NFont \"{i18nFontData.name}\" has null font for language \"{language}\"!");

                baseFont.fallbackFontAssetTable.Add(targetFont.Font);
            }

            // Add other language fonts.
            int count = i18nFontData.I18NFonts.Count;
            for (int i = 0; i < count; i++)
            {
                var i18nFont = i18nFontData.I18NFonts[i];
                if (baseFont.fallbackFontAssetTable.Contains(i18nFont.Font) == false)
                    baseFont.fallbackFontAssetTable.Add(i18nFont.Font);
            }

            if (forceRefreshAllTmpFonts)
                ForceRefreshAllTMPTextsForFallbackFonts();
        }

        /// <summary>
        /// Forces all <see cref="TMP_Text"/> components in the scene to refresh<br/>
        /// so that updated fallback font settings are correctly applied,<br/>
        /// including in build/runtime environments.
        /// </summary>
        public static void ForceRefreshAllTMPTextsForFallbackFonts()
        {
            // Clear fallback font glyph cache.
            TMP_ResourceManager.ClearFontAssetGlyphCache();

            // Force update TextMeshPro components mesh.
            // * Without this, font will does not updated in Build environment
            var tmpTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var tmpTextsCount = tmpTexts.Length;
            for (int i = 0; i < tmpTextsCount; i++)
            {
                var tmpText = tmpTexts[i];
                tmpText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            }
        }
    }
}
